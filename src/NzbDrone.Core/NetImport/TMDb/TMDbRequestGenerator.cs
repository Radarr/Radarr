using System;
using NzbDrone.Common.Http;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;

namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbRequestGenerator : INetImportRequestGenerator
    {
        public TMDbSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        public int MaxPages { get; set; }

        public TMDbRequestGenerator()
        {
            MaxPages = 3;
        }

        public virtual void Clean(NzbDrone.Core.Tv.Movie movie)
        {
            ;
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var minVoteCount = Settings.MinVotes;
            var minVoteAverage = Settings.MinVoteAverage;
            var ceritification = Settings.Ceritification;
            var includeGenreIds = Settings.IncludeGenreIds;
            var excludeGenreIds = Settings.ExcludeGenreIds;
            var languageCode = (TMDbLanguageCodes)Settings.LanguageCode;

            var todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            var threeMonthsAgo = DateTime.Parse(todaysDate).AddMonths(-3).ToString("yyyy-MM-dd");
            var threeMonthsFromNow = DateTime.Parse(todaysDate).AddMonths(3).ToString("yyyy-MM-dd");

            if (ceritification.IsNotNullOrWhiteSpace())
            {
                ceritification = $"&certification_country=US&certification={ceritification}";
            }

            var tmdbParams = "";
            switch (Settings.ListType)
            {
                case (int)TMDbListType.List:
                    tmdbParams = $"/3/list/{Settings.ListId}?api_key=1a7373301961d03f97f853a876dd1212";
                    break;
                case (int)TMDbListType.Theaters:
                    tmdbParams = $"/3/discover/movie?api_key=1a7373301961d03f97f853a876dd1212&primary_release_date.gte={threeMonthsAgo}&primary_release_date.lte={todaysDate}&vote_count.gte={minVoteCount}&vote_average.gte={minVoteAverage}{ceritification}&with_genres={includeGenreIds}&without_genres={excludeGenreIds}&with_original_language={languageCode}";
                    break;
                case (int)TMDbListType.Popular:
                    tmdbParams = $"/3/discover/movie?api_key=1a7373301961d03f97f853a876dd1212&sort_by=popularity.desc&vote_count.gte={minVoteCount}&vote_average.gte={minVoteAverage}{ceritification}&with_genres={includeGenreIds}&without_genres={excludeGenreIds}&with_original_language={languageCode}";
                    break;
                case (int)TMDbListType.Top:
                    tmdbParams = $"/3/discover/movie?api_key=1a7373301961d03f97f853a876dd1212&sort_by=vote_average.desc&vote_count.gte={minVoteCount}&vote_average.gte={minVoteAverage}{ceritification}&with_genres={includeGenreIds}&without_genres={excludeGenreIds}&with_original_language={languageCode}";
                    break;
                case (int)TMDbListType.Upcoming:
                    tmdbParams = $"/3/discover/movie?api_key=1a7373301961d03f97f853a876dd1212&primary_release_date.gte={todaysDate}&primary_release_date.lte={threeMonthsFromNow}&vote_count.gte={minVoteCount}&vote_average.gte={minVoteAverage}{ceritification}&with_genres={includeGenreIds}&without_genres={excludeGenreIds}&with_original_language={languageCode}";
                    break;
            }

            var pageableRequests = new NetImportPageableRequestChain();
            if (Settings.ListType != (int)TMDbListType.List)
            {
                // First query to get the total_Pages
                var requestBuilder = new HttpRequestBuilder($"{Settings.Link.TrimEnd("/")}")
                {
                    LogResponseContent = true
                };

                requestBuilder.Method = HttpMethod.GET;
                requestBuilder.Resource(tmdbParams);

                var request = requestBuilder
                    // .AddQueryParam("api_key", "1a7373301961d03f97f853a876dd1212")
                    .Accept(HttpAccept.Json)
                    .Build();

                var response = HttpClient.Execute(request);
                var result = Json.Deserialize<MovieSearchRoot>(response.Content);

                // @TODO Prolly some error handling to do here
                pageableRequests.Add(GetMovies(tmdbParams, result.total_pages));
                return pageableRequests;
            }

            pageableRequests.Add(GetMovies(tmdbParams, 0));
            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMovies(string tmdbParams, int totalPages)
        {
            var baseUrl = $"{Settings.Link.TrimEnd("/")}{tmdbParams}";
            if (Settings.ListType != (int)TMDbListType.List)
            {
                for (var pageNumber = 1; pageNumber <= totalPages; pageNumber++)
                {
                    // Limit the amount of pages
                    if (pageNumber >= MaxPages + 1)
                    {
                        Logger.Info(
                            $"Found more than {MaxPages} pages, skipping the {totalPages - (MaxPages + 1)} remaining pages");
                        break;
                    }

                    Logger.Info($"Importing TMDb movies from: {baseUrl}&page={pageNumber}");
                    yield return new NetImportRequest($"{baseUrl}&page={pageNumber}", HttpAccept.Json);
                }
            }
            else
            {
                Logger.Info($"Importing TMDb movies from: {baseUrl}");
                yield return new NetImportRequest($"{baseUrl}", HttpAccept.Json);
            }

        }
    }
}