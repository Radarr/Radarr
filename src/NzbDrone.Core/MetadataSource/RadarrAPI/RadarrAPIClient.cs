using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.MetadataSource.RadarrAPI
{
    public interface IRadarrAPIClient
    {
        IHttpRequestBuilderFactory RadarrAPI { get; }
        List<MovieResult> DiscoverMovies(string action, Func<HttpRequest, HttpRequest> enhanceRequest);
        List<AlternativeTitle> AlternativeTitlesForMovie(int TmdbId);
        Tuple<List<AlternativeTitle>, AlternativeYear> AlternativeTitlesAndYearForMovie(int tmdbId);
        AlternativeTitle AddNewAlternativeTitle(AlternativeTitle title, int TmdbId);
        AlternativeYear AddNewAlternativeYear(int year, int tmdbId);
        string APIURL { get; }
    }

    public class RadarrAPIClient : IRadarrAPIClient
    {
        private readonly IHttpClient _httpClient;

        public string APIURL { get; private set; }

        public RadarrAPIClient(IConfigFileProvider configFile, IHttpClient httpClient)
        {
            _httpClient = httpClient;

            APIURL = "https://api.radarr.video/v2";

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

                if (error != null && error.Errors != null && error.Errors.Count != 0)
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


        public List<AlternativeTitle> AlternativeTitlesForMovie(int TmdbId)
        {
            var request = RadarrAPI.Create().SetSegment("route", "mappings").SetSegment("action", "find").AddQueryParam("tmdbid", TmdbId).Build();

            var mappings = Execute<Mapping>(request);

            var titles = new List<NzbDrone.Core.Movies.AlternativeTitles.AlternativeTitle>();

            foreach (var altTitle in mappings.Mappings.Titles)
            {
                titles.Add(new NzbDrone.Core.Movies.AlternativeTitles.AlternativeTitle(altTitle.Info.AkaTitle, SourceType.Mappings, altTitle.Id));
            }

            return titles;
        }
        
        public Tuple<List<AlternativeTitle>, AlternativeYear> AlternativeTitlesAndYearForMovie(int tmdbId)
        {
            var request = RadarrAPI.Create().SetSegment("route", "mappings").SetSegment("action", "find").AddQueryParam("tmdbid", tmdbId).Build();

            var mappings = Execute<Mapping>(request);

            var titles = new List<NzbDrone.Core.Movies.AlternativeTitles.AlternativeTitle>();

            foreach (var altTitle in mappings.Mappings.Titles)
            {
                titles.Add(new NzbDrone.Core.Movies.AlternativeTitles.AlternativeTitle(altTitle.Info.AkaTitle, SourceType.Mappings, altTitle.Id));
            }

            var year = mappings.Mappings.Years.Where(y => y.Votes >= 3).OrderBy(y => y.Votes).FirstOrDefault();

            AlternativeYear newYear = null;

            if (year != null)
            {
                newYear = new AlternativeYear
                {
                    Year = year.Info.AkaYear,
                    SourceId = year.Id
                };
            }
            
            return new Tuple<List<AlternativeTitle>, AlternativeYear>(titles, newYear);
        }

        public AlternativeTitle AddNewAlternativeTitle(AlternativeTitle title, int TmdbId)
        {
            var request = RadarrAPI.Create().SetSegment("route", "mappings").SetSegment("action", "add")
                .AddQueryParam("tmdbid", TmdbId).AddQueryParam("type", "title")
                .AddQueryParam("language", IsoLanguages.Get(title.Language).TwoLetterCode)
                .AddQueryParam("aka_title", title.Title).Build();

            var newMapping = Execute<AddTitleMapping>(request);
            
            var newTitle = new AlternativeTitle(newMapping.Info.AkaTitle, SourceType.Mappings, newMapping.Id, title.Language);
            newTitle.VoteCount = newMapping.VoteCount;
            newTitle.Votes = newMapping.Votes;
            
            return newTitle;
        }

        public AlternativeYear AddNewAlternativeYear(int year, int tmdbId)
        {
            var request = RadarrAPI.Create().SetSegment("route", "mappings").SetSegment("action", "add")
                .AddQueryParam("tmdbid", tmdbId).AddQueryParam("type", "year")
                .AddQueryParam("aka_year", year).Build();

            var newYear = Execute<AddYearMapping>(request);

            return new AlternativeYear
            {
                Year = newYear.Info.AkaYear,
                SourceId = newYear.Id
            };
        }
        
        public IHttpRequestBuilderFactory RadarrAPI { get; private set; }
    }
}
