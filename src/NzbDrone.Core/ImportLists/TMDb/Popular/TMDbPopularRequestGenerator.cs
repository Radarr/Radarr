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
            var languageCode = Settings.FilterCriteria.LanguageCode;

            var now = DateTime.UtcNow;
            var todaysDate = now.ToString("yyyy-MM-dd");
            var threeMonthsAgo = now.AddMonths(-3).ToString("yyyy-MM-dd");
            var threeMonthsFromNow = now.AddMonths(3).ToString("yyyy-MM-dd");

            var requestBuilder = RequestBuilder.Create()
                .SetSegment("api", "3")
                .SetSegment("route", "discover")
                .SetSegment("id", "")
                .SetSegment("secondaryRoute", "movie")
                .Accept(HttpAccept.Json);

            switch (Settings.TMDbListType)
            {
                case (int)TMDbPopularListType.Theaters:
                    requestBuilder
                        .AddQueryParam("primary_release_date.gte", threeMonthsAgo)
                        .AddQueryParam("primary_release_date.lte", todaysDate);
                    break;
                case (int)TMDbPopularListType.Popular:
                    requestBuilder.AddQueryParam("sort_by", "popularity.desc");
                    break;
                case (int)TMDbPopularListType.Top:
                    requestBuilder.AddQueryParam("sort_by", "vote_average.desc");
                    break;
                case (int)TMDbPopularListType.Upcoming:
                    requestBuilder
                        .AddQueryParam("primary_release_date.gte", todaysDate)
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

            if (languageCode.HasValue)
            {
                requestBuilder.AddQueryParam("with_original_language", (TMDbLanguageCodes)languageCode);
            }

            for (var pageNumber = 1; pageNumber <= MaxPages; pageNumber++)
            {
                requestBuilder.AddQueryParam("page", pageNumber, true);

                var request = requestBuilder.Build();

                if (pageNumber == 1 || pageNumber == MaxPages)
                {
                    Logger.Debug("Processing TMDb Popular page {0} of {1}", pageNumber, MaxPages);
                }

                yield return new ImportListRequest(request);
            }
        }
    }
}
