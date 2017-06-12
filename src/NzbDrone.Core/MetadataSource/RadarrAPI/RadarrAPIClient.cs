using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;

namespace NzbDrone.Core.MetadataSource.RadarrAPI
{
    public interface IRadarrAPIClient
    {
        IHttpRequestBuilderFactory RadarrAPI { get; }
        List<MovieResult> DiscoverMovies(string action, Func<HttpRequest, HttpRequest> enhanceRequest);
        string APIURL { get; }
    }

    public class RadarrAPIClient : IRadarrAPIClient
    {
        private readonly IHttpClient _httpClient;

        public string APIURL { get; private set; }

        public RadarrAPIClient(IConfigFileProvider configFile, IHttpClient httpClient)
        {
            _httpClient = httpClient;

            if (configFile.Branch == "nightly")
            {
                APIURL = "https://staging.api.radarr.video";
            }
            else
            {
                APIURL = "https://api.radarr.video/v2";
            }

            RadarrAPI = new HttpRequestBuilder(APIURL+"/{route}/{action}")
                                                        .CreateFactory();
        }

        private HttpResponse Execute(HttpRequest request)
        {
            if (request.Method == HttpMethod.GET)
            {
                return _httpClient.Get(request);
            }
            else if (request.Method == HttpMethod.POST)
            {
                return _httpClient.Post(request);
            }
            else
            {
                throw new NotImplementedException($"Method {request.Method} not implemented");
            }
        }

        private T Execute<T>(HttpRequest request)
        {
            request.AllowAutoRedirect = true;
            request.Headers.Accept = HttpAccept.Json.Value;
            request.SuppressHttpError = true;

            var response = Execute(request);

            try
            {
                var error = JsonConvert.DeserializeObject<RadarrError>(response.Content);

                if (error != null && error.Errors.Count != 0)
                {
                    throw new RadarrAPIException(error);
                }
            }
            catch (JsonSerializationException)
            {
                //No error!
            }


            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpException(request, response);
            }

            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public List<MovieResult> DiscoverMovies(string action, Func<HttpRequest, HttpRequest> enhanceRequest = null )
        {
            var request = RadarrAPI.Create().SetSegment("route", "discovery").SetSegment("action", action).Build();

            if (enhanceRequest != null)
            {
                request = enhanceRequest(request);
            }

            return Execute<List<MovieResult>>(request);
        }

        public IHttpRequestBuilderFactory RadarrAPI { get; private set; }
    }
}
