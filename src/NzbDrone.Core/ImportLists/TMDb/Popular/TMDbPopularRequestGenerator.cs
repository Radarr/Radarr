using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.Popular
{
    public class TMDbPopularRequestGenerator : IImportListRequestGenerator
    {
        public TMDbPopularSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }

        public int MaxPages { get; set; }

        public TMDbPopularRequestGenerator()
        {
            MaxPages = 3;
        }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequests());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequests()
        {
            var minVoteCount = Settings.FilterCriteria.MinVotes;
            var minVoteAverage = Settings.FilterCriteria.MinVoteAverage;
            var certification = Settings.FilterCriteria.Certification;
            var includeGenreIds = Settings.FilterCriteria.IncludeGenreIds;
            var excludeGenreIds = Settings.FilterCriteria.ExcludeGenreIds;
            var includeCompanyIds = Settings.FilterCriteria.IncludeCompanyIds;
            var excludeCompanyIds = Settings.FilterCriteria.ExcludeCompanyIds;
            var languageCode = (TMDbLanguageCodes)Settings.FilterCriteria.LanguageCode;

            var todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            var threeMonthsAgo = DateTime.Parse(todaysDate).AddMonths(-3).ToString("yyyy-MM-dd");
            var threeMonthsFromNow = DateTime.Parse(todaysDate).AddMonths(3).ToString("yyyy-MM-dd");

            var requestBuilder = RequestBuilder.Create()
                                               .SetSegment("api", "3")
                                               .SetSegment("route", "discover")
                                               .SetSegment("id", "")
                                               .SetSegment("secondaryRoute", "movie");

            switch (Settings.TMDbListType)
            {
                case (int)TMDbPopularListType.Theaters:
                    requestBuilder.AddQueryParam("primary_release_date.gte", threeMonthsAgo)
                                  .AddQueryParam("primary_release_date.lte", todaysDate);
                    break;
                case (int)TMDbPopularListType.Popular:
                    requestBuilder.AddQueryParam("sort_by", "popularity.desc");
                    break;
                case (int)TMDbPopularListType.Top:
                    requestBuilder.AddQueryParam("sort_by", "vote_average.desc");
                    break;
                case (int)TMDbPopularListType.Upcoming:
                    requestBuilder.AddQueryParam("primary_release_date.gte", todaysDate)
                                  .AddQueryParam("primary_release_date.lte", threeMonthsFromNow);
                    break;
            }

            if (certification.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("certification", certification)
                              .AddQueryParam("certification_country", "US");
            }

            if (minVoteCount.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("vote_count.gte", minVoteCount);
            }

            if (minVoteAverage.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("vote_average.gte", minVoteAverage);
            }

            if (includeGenreIds.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("with_genres", includeGenreIds);
            }

            if (excludeGenreIds.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("without_genres", excludeGenreIds);
            }

            if (includeCompanyIds.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("with_companies", includeCompanyIds);
            }

            if (excludeCompanyIds.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("without_companies", excludeCompanyIds);
            }

            requestBuilder
                .AddQueryParam("with_original_language", languageCode)
                .Accept(HttpAccept.Json);

            for (var pageNumber = 1; pageNumber <= MaxPages; pageNumber++)
            {
                Logger.Info($"Importing TMDb movies from: {requestBuilder.BaseUrl}&page={pageNumber}");

                requestBuilder.AddQueryParam("page", pageNumber, true);

                yield return new ImportListRequest(requestBuilder.Build());
            }
        }
    }
}
