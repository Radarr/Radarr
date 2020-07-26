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
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport.Exceptions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport
{
    public abstract class HttpNetImportBase<TSettings> : NetImportBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected readonly IHttpClient _httpClient;

        public override bool Enabled => true;
        public bool SupportsPaging => PageSize > 20;

        public virtual int PageSize => 20;
        public virtual TimeSpan RateLimit => TimeSpan.FromSeconds(2);

        public abstract INetImportRequestGenerator GetRequestGenerator();
        public abstract IParseNetImportResponse GetParser();

        protected HttpNetImportBase(IHttpClient httpClient, INetImportStatusService netImportStatusService,  IConfigService configService, IParsingService parsingService, Logger logger)
            : base(netImportStatusService, configService, parsingService, logger)
        {
            _httpClient = httpClient;
        }

        public override NetImportFetchResult Fetch()
        {
            var generator = GetRequestGenerator();
            return FetchMovies(generator.GetMovies());
        }

        protected virtual NetImportFetchResult FetchMovies(NetImportPageableRequestChain pageableRequestChain, bool isRecent = false)
        {
            var movies = new List<Movie>();
            var url = string.Empty;

            var parser = GetParser();

            var anyFailure = true;

            try
            {
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
                        }

                        movies.AddRange(pagedReleases);
                    }

                    if (movies.Any())
                    {
                        break;
                    }
                }

                _netImportStatusService.RecordSuccess(Definition.Id);
                anyFailure = false;
            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.NameResolutionFailure ||
                    webException.Status == WebExceptionStatus.ConnectFailure)
                {
                    _netImportStatusService.RecordConnectionFailure(Definition.Id);
                }
                else
                {
                    _netImportStatusService.RecordFailure(Definition.Id);
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
            catch (TooManyRequestsException ex)
            {
                if (ex.RetryAfter != TimeSpan.Zero)
                {
                    _netImportStatusService.RecordFailure(Definition.Id, ex.RetryAfter);
                }
                else
                {
                    _netImportStatusService.RecordFailure(Definition.Id, TimeSpan.FromHours(1));
                }

                _logger.Warn("API Request Limit reached for {0}", this);
            }
            catch (HttpException ex)
            {
                _netImportStatusService.RecordFailure(Definition.Id);
                _logger.Warn("{0} {1}", this, ex.Message);
            }
            catch (RequestLimitReachedException)
            {
                _netImportStatusService.RecordFailure(Definition.Id, TimeSpan.FromHours(1));
                _logger.Warn("API Request Limit reached for {0}", this);
            }
            catch (CloudFlareCaptchaException ex)
            {
                _netImportStatusService.RecordFailure(Definition.Id);
                ex.WithData("FeedUrl", url);
                if (ex.IsExpired)
                {
                    _logger.Error(ex, "Expired CAPTCHA token for {0}, please refresh in import list settings.", this);
                }
                else
                {
                    _logger.Error(ex, "CAPTCHA token required for {0}, check import list settings.", this);
                }
            }
            catch (NetImportException ex)
            {
                _netImportStatusService.RecordFailure(Definition.Id);
                _logger.Warn(ex, "{0}", url);
            }
            catch (Exception ex)
            {
                _netImportStatusService.RecordFailure(Definition.Id);
                ex.WithData("FeedUrl", url);
                _logger.Error(ex, "An error occurred while processing feed. {0}", url);
            }

            return new NetImportFetchResult { Movies = movies, AnyFailure = anyFailure };
        }

        protected virtual IList<Movie> FetchPage(NetImportRequest request, IParseNetImportResponse parser)
        {
            var response = FetchNetImportResponse(request);

            return parser.ParseResponse(response).ToList().Select(m =>
            {
                m.RootFolderPath = ((NetImportDefinition)Definition).RootFolderPath;
                m.ProfileId = ((NetImportDefinition)Definition).ProfileId;
                m.Monitored = ((NetImportDefinition)Definition).ShouldMonitor;
                m.MinimumAvailability = ((NetImportDefinition)Definition).MinimumAvailability;
                m.Tags = ((NetImportDefinition)Definition).Tags;
                return m;
            }).ToList();
        }

        protected virtual NetImportResponse FetchNetImportResponse(NetImportRequest request)
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
            catch (RequestLimitReachedException)
            {
                _logger.Warn("Request limit reached");
            }
            catch (UnsupportedFeedException ex)
            {
                _logger.Warn(ex, "Net Import feed is not supported");

                return new ValidationFailure(string.Empty, "Net Import feed is not supported: " + ex.Message);
            }
            catch (NetImportException ex)
            {
                _logger.Warn(ex, "Unable to connect to list");

                return new ValidationFailure(string.Empty, "Unable to connect to list. " + ex.Message);
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
