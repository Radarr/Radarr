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
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists
{
    public abstract class HttpImportListBase<TSettings> : ImportListBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected readonly IHttpClient _httpClient;

        public override bool Enabled => true;
        public bool SupportsPaging => PageSize > 20;

        public virtual int PageSize => 20;
        public virtual TimeSpan RateLimit => TimeSpan.FromSeconds(2);

        public abstract IImportListRequestGenerator GetRequestGenerator();
        public abstract IParseImportListResponse GetParser();

        protected HttpImportListBase(IHttpClient httpClient, IImportListStatusService importListStatusService,  IConfigService configService, IParsingService parsingService, Logger logger)
            : base(importListStatusService, configService, parsingService, logger)
        {
            _httpClient = httpClient;
        }

        public override ImportListFetchResult Fetch()
        {
            var generator = GetRequestGenerator();
            return FetchMovies(generator.GetMovies());
        }

        protected virtual ImportListFetchResult FetchMovies(ImportListPageableRequestChain pageableRequestChain, bool isRecent = false)
        {
            var movies = new List<ImportListMovie>();
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
                        var pagedReleases = new List<ImportListMovie>();
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

                _importListStatusService.RecordSuccess(Definition.Id);
                anyFailure = false;
            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.NameResolutionFailure ||
                    webException.Status == WebExceptionStatus.ConnectFailure)
                {
                    _importListStatusService.RecordConnectionFailure(Definition.Id);
                }
                else
                {
                    _importListStatusService.RecordFailure(Definition.Id);
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
                    _importListStatusService.RecordFailure(Definition.Id, ex.RetryAfter);
                }
                else
                {
                    _importListStatusService.RecordFailure(Definition.Id, TimeSpan.FromHours(1));
                }

                _logger.Warn("API Request Limit reached for {0}", this);
            }
            catch (HttpException ex)
            {
                _importListStatusService.RecordFailure(Definition.Id);
                _logger.Warn("{0} {1}", this, ex.Message);
            }
            catch (RequestLimitReachedException)
            {
                _importListStatusService.RecordFailure(Definition.Id, TimeSpan.FromHours(1));
                _logger.Warn("API Request Limit reached for {0}", this);
            }
            catch (CloudFlareCaptchaException ex)
            {
                _importListStatusService.RecordFailure(Definition.Id);
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
            catch (ImportListException ex)
            {
                _importListStatusService.RecordFailure(Definition.Id);
                _logger.Warn(ex, "{0}", url);
            }
            catch (Exception ex)
            {
                _importListStatusService.RecordFailure(Definition.Id);
                ex.WithData("FeedUrl", url);
                _logger.Error(ex, "An error occurred while processing feed. {0}", url);
            }

            return new ImportListFetchResult { Movies = CleanupListItems(movies), AnyFailure = anyFailure };
        }

        protected virtual IList<ImportListMovie> FetchPage(ImportListRequest request, IParseImportListResponse parser)
        {
            var response = FetchImportListResponse(request);

            return parser.ParseResponse(response).ToList();
        }

        protected virtual ImportListResponse FetchImportListResponse(ImportListRequest request)
        {
            _logger.Debug("Downloading List " + request.HttpRequest.ToString(false));

            if (request.HttpRequest.RateLimit < RateLimit)
            {
                request.HttpRequest.RateLimit = RateLimit;
            }

            request.HttpRequest.AllowAutoRedirect = true;

            return new ImportListResponse(request, _httpClient.Execute(request.HttpRequest));
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
                    return new NzbDroneValidationFailure(string.Empty,
                               "No results were returned from your import list, please check your settings and the log for details.")
                    { IsWarning = true };
                }
            }
            catch (RequestLimitReachedException)
            {
                _logger.Warn("Request limit reached");
            }
            catch (UnsupportedFeedException ex)
            {
                _logger.Warn(ex, "Import list feed is not supported");

                return new ValidationFailure(string.Empty, "Import list feed is not supported: " + ex.Message);
            }
            catch (ImportListException ex)
            {
                _logger.Warn(ex, "Unable to connect to list");

                return new ValidationFailure(string.Empty, $"Unable to connect to import list: {ex.Message}. Check the log surrounding this error for details.");
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to import list");

                return new ValidationFailure(string.Empty, $"Unable to connect to import list: {ex.Message}. Check the log surrounding this error for details.");
            }

            return null;
        }
    }
}
