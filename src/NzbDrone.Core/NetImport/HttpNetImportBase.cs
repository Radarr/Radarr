using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
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

        public HttpNetImportBase(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(configService, parsingService, logger)
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

            var anyFailure = false;

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
            }
            catch (WebException webException)
            {
                anyFailure = true;
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
                anyFailure = true;
                if ((int)httpException.Response.StatusCode == 429)
                {
                    _logger.Warn("API Request Limit reached for {0}", this);
                }
                else
                {
                    _logger.Warn("{0} {1}", this, httpException.Message);
                }
            }
            catch (Exception feedEx)
            {
                anyFailure = true;
                feedEx.Data.Add("FeedUrl", url);
                _logger.Error(feedEx, "An error occurred while processing list feed {0}", url);
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
