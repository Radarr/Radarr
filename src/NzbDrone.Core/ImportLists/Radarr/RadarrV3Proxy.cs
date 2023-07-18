using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Radarr
{
    public interface IRadarrV3Proxy
    {
        List<RadarrMovie> GetMovies(RadarrSettings settings);
        List<RadarrProfile> GetProfiles(RadarrSettings settings);
        List<RadarrRootFolder> GetRootFolders(RadarrSettings settings);
        List<RadarrTag> GetTags(RadarrSettings settings);
        ValidationFailure Test(RadarrSettings settings);
    }

    public class RadarrV3Proxy : IRadarrV3Proxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public RadarrV3Proxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public List<RadarrMovie> GetMovies(RadarrSettings settings)
        {
            var requestBuilder = BuildRequest("/api/v3/movie", settings);

            requestBuilder.AddQueryParam("excludeLocalCovers", true);

            return Execute<RadarrMovie>(requestBuilder, settings);
        }

        public List<RadarrProfile> GetProfiles(RadarrSettings settings)
        {
            return Execute<RadarrProfile>(BuildRequest("/api/v3/qualityprofile", settings), settings);
        }

        public List<RadarrRootFolder> GetRootFolders(RadarrSettings settings)
        {
            return Execute<RadarrRootFolder>(BuildRequest("api/v3/rootfolder", settings), settings);
        }

        public List<RadarrTag> GetTags(RadarrSettings settings)
        {
            return Execute<RadarrTag>(BuildRequest("/api/v3/tag", settings), settings);
        }

        public ValidationFailure Test(RadarrSettings settings)
        {
            try
            {
                GetMovies(settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "API Key is invalid");
                    return new ValidationFailure("ApiKey", "API Key is invalid");
                }

                if (ex.Response.HasHttpRedirect)
                {
                    _logger.Error(ex, "Radarr returned redirect and is invalid");
                    return new ValidationFailure("BaseUrl", "Radarr URL is invalid, are you missing a URL base?");
                }

                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure(string.Empty, $"Unable to connect to import list: {ex.Message}. Check the log surrounding this error for details.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure(string.Empty, $"Unable to connect to import list: {ex.Message}. Check the log surrounding this error for details.");
            }

            return null;
        }

        private HttpRequestBuilder BuildRequest(string resource, RadarrSettings settings)
        {
            var baseUrl = settings.BaseUrl.TrimEnd('/');

            return new HttpRequestBuilder(baseUrl).Resource(resource)
                .Accept(HttpAccept.Json)
                .SetHeader("X-Api-Key", settings.ApiKey);
        }

        private List<TResource> Execute<TResource>(HttpRequestBuilder requestBuilder, RadarrSettings settings)
        {
            if (settings.BaseUrl.IsNullOrWhiteSpace() || settings.ApiKey.IsNullOrWhiteSpace())
            {
                return new List<TResource>();
            }

            var request = requestBuilder.Build();

            var response = _httpClient.Get(request);

            if ((int)response.StatusCode >= 300)
            {
                throw new HttpException(response);
            }

            var results = JsonConvert.DeserializeObject<List<TResource>>(response.Content);

            return results;
        }
    }
}
