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
        private const string ApiKey = "1a7373301961d03f97f853a876dd1212";
        public TMDbSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        public int MaxPages { get; set; }

        public TMDbRequestGenerator()
        {
            MaxPages = 3;
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var minVoteCount = Settings.MinVotes;
            var minVoteAverage = Settings.MinVoteAverage;
            var ceritification = Settings.Ceritification;
            var includeGenreIds = Settings.IncludeGenreIds;
            var excludeGenreIds = Settings.ExcludeGenreIds;
            var languageCode = (TMDbLanguageCodes) Settings.LanguageCode;
            var todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            var threeMonthsAgo = DateTime.Parse(todaysDate).AddMonths(-3).ToString("yyyy-MM-dd");
            var threeMonthsFromNow = DateTime.Parse(todaysDate).AddMonths(3).ToString("yyyy-MM-dd");
            if (ceritification.IsNotNullOrWhiteSpace())
            {
                ceritification = $"&certification_country=US&certification={ceritification}";
            }

            var requestBuilder = new HttpRequestBuilder(Settings.Link.TrimEnd("/"))
                {
                    LogResponseContent = true
                }
                .AddQueryParam("api_key", ApiKey);
            switch (Settings.ListType)
            {
                case (int) TMDbListType.List:
                    requestBuilder = requestBuilder.Resource($"/3/list/{Settings.ListId}");
                    break;
                case (int) TMDbListType.Collection:
                    requestBuilder = requestBuilder.Resource($"/3/collection/{Settings.ListId}");
                    break;
                case (int) TMDbListType.Theaters:
                    requestBuilder = requestBuilder.Resource("/3/discover/movie")
                        .AddQueryParam("primary_release.gte", threeMonthsAgo)
                        .AddQueryParam("primary_release_date.lte", todaysDate);
                    break;
                case (int) TMDbListType.Popular:
                    requestBuilder = requestBuilder.Resource("/3/discover/movie")
                        .AddQueryParam("sort_by", "popularity.desc")
                        .AddQueryParam("vote_count.gte", minVoteCount);
                    break;
                case (int) TMDbListType.Top:
                    requestBuilder = requestBuilder.Resource("/3/discover/movie")
                        .AddQueryParam("sort_by", "vote_average.desc")
                        .AddQueryParam("vote_count.gte", minVoteCount);
                    break;
                case (int) TMDbListType.Upcoming:
                    requestBuilder = requestBuilder.Resource("/3/discover/movie")
                        .AddQueryParam("primary_release.gte", todaysDate)
                        .AddQueryParam("primary_release_date.lte", threeMonthsFromNow);
                    break;
            }

            var pageableRequests = new NetImportPageableRequestChain();
            var totalPages = 0;
            switch (Settings.ListType)
            {
                case (int) TMDbListType.List:
                case (int) TMDbListType.Collection:
                    break;
                default:
                    requestBuilder = requestBuilder
                        .AddQueryParam("vote_count.gte", minVoteCount)
                        .AddQueryParam("vote_average.gte", minVoteAverage)
                        .AddQueryParam("with_genres", includeGenreIds)
                        .AddQueryParam("without_genres", excludeGenreIds)
                        .AddQueryParam("certification_country", "US")
                        .AddQueryParam("certification", ceritification)
                        .AddQueryParam("with_original_language", languageCode)
                        .Accept(HttpAccept.Json);
                    var request = requestBuilder.Build();
                    request.Method = HttpMethod.GET;
                    var response = HttpClient.Execute(request);
                    var result = Json.Deserialize<MovieSearchRoot>(response.Content);
                    totalPages = result.total_pages;
                    break;
            }

            var url = requestBuilder.Build().Url.ToString();
            pageableRequests.Add(GetMovies(requestBuilder.Build().Url.ToString(), totalPages));
            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMovies(string url, int totalPages)
        {
            switch (Settings.ListType)
            {
                case (int) TMDbListType.List:
                case (int) TMDbListType.Collection:
                {
                    Logger.Info($"Importing TMDb movies from: {url}");
                    yield return new NetImportRequest($"{url}", HttpAccept.Json);
                    break;
                }

                default:
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

                        Logger.Info($"Importing TMDb movies from: {url}&page={pageNumber}");
                        yield return new NetImportRequest($"{url}&page={pageNumber}", HttpAccept.Json);
                    }

                    break;
                }
            }
        }
    }
}
