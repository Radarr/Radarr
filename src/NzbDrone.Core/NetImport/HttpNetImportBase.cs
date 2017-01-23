using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Http.CloudFlare;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.NetImport.Exceptions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport
{
    public abstract class HttpNetImportBase<TSettings> : NetImportBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected const int MaxNumResultsPerQuery = 1000;

        protected readonly IHttpClient _httpClient;

        public override bool Enabled => true;

        public bool SupportsPaging => PageSize > 0;

        public virtual int PageSize => 0;

        public virtual TimeSpan RateLimit => TimeSpan.FromSeconds(2);

        public abstract INetImportRequestGenerator GetRequestGenerator();
        public abstract IParseNetImportResponse GetParser();

        public HttpNetImportBase(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(configService, parsingService, logger)
        {
            _httpClient = httpClient;
        }

        public override IList<Movie> Fetch()
        {
            var generator = GetRequestGenerator();
            
            return FetchMovies(generator.GetMovies());
        }

        protected virtual IList<Movie> FetchMovies(NetImportPageableRequestChain pageableRequestChain, bool isRecent = false)
        {
            var movies = new List<Movie>();
            var url = string.Empty;

            var parser = GetParser();

            try
            {
                var fullyUpdated = false;
                Movie lastMovie = null;
                if (isRecent)
                {
                    //lastReleaseInfo = _indexerStatusService.GetLastRssSyncReleaseInfo(Definition.Id);
                }

                for (int i = 0; i < pageableRequestChain.Tiers; i++)
                {
                    var pageableRequests = pageableRequestChain.GetTier(i);

                    foreach (var pageableRequest in pageableRequests)
                    {
                        var pagedReleases = new List<Movie>();

                        foreach (var request in pageableRequest)
                        {
                            url = request.Url.FullUri;

                            var page = FetchPage(request, parser);

                            pagedReleases.AddRange(page);

                            if (isRecent && page.Any())
                            {
                                if (lastMovie == null)
                                {
                                    fullyUpdated = true;
                                    break;
                                }/*
                                var oldestReleaseDate = page.Select(v => v.PublishDate).Min();
                                if (oldestReleaseDate < lastReleaseInfo.PublishDate || page.Any(v => v.DownloadUrl == lastReleaseInfo.DownloadUrl))
                                {
                                    fullyUpdated = true;
                                    break;
                                }

                                if (pagedReleases.Count >= MaxNumResultsPerQuery &&
                                    oldestReleaseDate < DateTime.UtcNow - TimeSpan.FromHours(24))
                                {
                                    fullyUpdated = false;
                                    break;
                                }*///update later
                            }
                            else if (pagedReleases.Count >= MaxNumResultsPerQuery)
                            {
                                break;
                            }

                            if (!IsFullPage(page))
                            {
                                break;
                            }
                        }

                        movies.AddRange(pagedReleases);
                    }

                    if (movies.Any())
                    {
                        break;
                    }
                }

                if (isRecent && !movies.Empty())
                {
                    var ordered = movies.OrderByDescending(v => v.Title).ToList();

                    lastMovie = ordered.First();
                    //_indexerStatusService.UpdateRssSyncStatus(Definition.Id, lastReleaseInfo);
                }

                //_indexerStatusService.RecordSuccess(Definition.Id);
            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.NameResolutionFailure ||
                    webException.Status == WebExceptionStatus.ConnectFailure)
                {
                    //_indexerStatusService.RecordConnectionFailure(Definition.Id);
                }
                else
                {
                    //_indexerStatusService.RecordFailure(Definition.Id);
                }

                if (webException.Message.Contains("502") || webException.Message.Contains("503") ||
                    webException.Message.Contains("timed out"))
                {
                    _logger.Warn("{0} server is currently unavailable. {1} {2}", this, url, webException.Message);
                }
                else
                {
                    _logger.Warn("{0} {1} {2}", this, url, webException.Message);
                }
            }
            catch (HttpException httpException)
            {
                if ((int)httpException.Response.StatusCode == 429)
                {
                    //_indexerStatusService.RecordFailure(Definition.Id, TimeSpan.FromHours(1));
                    _logger.Warn("API Request Limit reached for {0}", this);
                }
                else
                {
                    //_indexerStatusService.RecordFailure(Definition.Id);
                    _logger.Warn("{0} {1}", this, httpException.Message);
                }
            }
            catch (RequestLimitReachedException)
            {
                //_indexerStatusService.RecordFailure(Definition.Id, TimeSpan.FromHours(1));
                _logger.Warn("API Request Limit reached for {0}", this);
            }
            catch (ApiKeyException)
            {
                //_indexerStatusService.RecordFailure(Definition.Id);
                _logger.Warn("Invalid API Key for {0} {1}", this, url);
            }
            catch (CloudFlareCaptchaException ex)
            {
                //_indexerStatusService.RecordFailure(Definition.Id);
                if (ex.IsExpired)
                {
                    _logger.Error(ex, "Expired CAPTCHA token for {0}, please refresh in indexer settings.", this);
                }
                else
                {
                    _logger.Error(ex, "CAPTCHA token required for {0}, check indexer settings.", this);
                }
            }
            catch (IndexerException ex)
            {
                //_indexerStatusService.RecordFailure(Definition.Id);
                var message = string.Format("{0} - {1}", ex.Message, url);
                _logger.Warn(ex, message);
            }
            catch (Exception feedEx)
            {
                //_indexerStatusService.RecordFailure(Definition.Id);
                feedEx.Data.Add("FeedUrl", url);
                _logger.Error(feedEx, "An error occurred while processing feed. " + url);
            }

            return movies;
        }

        protected virtual bool IsFullPage(IList<Movie> page)
        {
            return PageSize != 0 && page.Count >= PageSize;
        }

        protected virtual IList<Movie> FetchPage(NetImportRequest request, IParseNetImportResponse parser)
        {
            var response = FetchIndexerResponse(request);

            return parser.ParseResponse(response).ToList().Select(m =>
            {
                m.RootFolderPath = ((NetImportDefinition) Definition).RootFolderPath;
                m.ProfileId = ((NetImportDefinition) Definition).ProfileId;
                m.Monitored = ((NetImportDefinition) Definition).ShouldMonitor;
                return m;
            }).ToList();
        }

        protected virtual NetImportResponse FetchIndexerResponse(NetImportRequest request)
        {
            _logger.Debug("Downloading List " + request.HttpRequest.ToString(false));

            if (request.HttpRequest.RateLimit < RateLimit)
            {
                request.HttpRequest.RateLimit = RateLimit;
            }

            request.HttpRequest.AllowAutoRedirect = true;

            return new NetImportResponse(request, _httpClient.Execute(request.HttpRequest));
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
        }

        protected virtual ValidationFailure TestConnection()
        {
            try
            {
                var parser = GetParser();
                var generator = GetRequestGenerator();
                var releases = FetchPage(generator.GetMovies().GetAllTiers().First().First(), parser);

                if (releases.Empty())
                {
                    return new ValidationFailure(string.Empty, "No results were returned from your list, please check your settings.");
                }
            }
            catch (ApiKeyException)
            {
                _logger.Warn("List returned result for RSS URL, API Key appears to be invalid");

                return new ValidationFailure("ApiKey", "Invalid API Key");
            }
            catch (RequestLimitReachedException)
            {
                _logger.Warn("Request limit reached");
            }
            catch (CloudFlareCaptchaException ex)
            {
                if (ex.IsExpired)
                {
                    return new ValidationFailure("CaptchaToken", "CloudFlare CAPTCHA token expired, please Refresh.");
                }
                else
                {
                    return new ValidationFailure("CaptchaToken", "Site protected by CloudFlare CAPTCHA. Valid CAPTCHA token required.");
                }
            }
            catch (UnsupportedFeedException ex)
            {
                _logger.Warn(ex, "List feed is not supported");

                return new ValidationFailure(string.Empty, "List feed is not supported: " + ex.Message);
            }
            catch (NetImportException ex)
            {
                _logger.Warn(ex, "Unable to connect to list");

                return new ValidationFailure(string.Empty, "Unable to connect to indexer. " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to list");

                return new ValidationFailure(string.Empty, "Unable to connect to list, check the log for more details");
            }

            return null;
        }
    }
    

}
