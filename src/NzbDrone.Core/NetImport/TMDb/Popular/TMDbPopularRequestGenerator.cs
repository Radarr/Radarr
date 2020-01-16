﻿using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.TMDb.Popular
{
    public class TMDbPopularRequestGenerator : INetImportRequestGenerator
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

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            pageableRequests.Add(GetMoviesRequests());

            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMoviesRequests()
        {
            var minVoteCount = Settings.FilterCriteria.MinVotes;
            var minVoteAverage = Settings.FilterCriteria.MinVoteAverage;
            var ceritification = Settings.FilterCriteria.Ceritification;
            var includeGenreIds = Settings.FilterCriteria.IncludeGenreIds;
            var excludeGenreIds = Settings.FilterCriteria.ExcludeGenreIds;
            var languageCode = (TMDbLanguageCodes)Settings.FilterCriteria.LanguageCode;

            var todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            var threeMonthsAgo = DateTime.Parse(todaysDate).AddMonths(-3).ToString("yyyy-MM-dd");
            var threeMonthsFromNow = DateTime.Parse(todaysDate).AddMonths(3).ToString("yyyy-MM-dd");

            var requestBuilder = RequestBuilder.Create()
                                               .SetSegment("api", "3")
                                               .SetSegment("route", "discover")
                                               .SetSegment("id", "")
                                               .SetSegment("secondaryRoute", "movie");

            switch (Settings.ListType)
            {
                case (int)TMDbPopularListType.Theaters:
                    requestBuilder.AddQueryParam("primary_release.gte", threeMonthsAgo)
                                  .AddQueryParam("primary_release_date.lte", todaysDate);
                    break;
                case (int)TMDbPopularListType.Popular:
                    requestBuilder.AddQueryParam("sort_by", "popularity.desc");
                    break;
                case (int)TMDbPopularListType.Top:
                    requestBuilder.AddQueryParam("sort_by", "vote_average.desc");
                    break;
                case (int)TMDbPopularListType.Upcoming:
                    requestBuilder.AddQueryParam("primary_release.gte", todaysDate)
                                  .AddQueryParam("primary_release_date.lte", threeMonthsFromNow);
                    break;
            }

            if (ceritification.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("certification", ceritification)
                              .AddQueryParam("certification_country", "US");
            }

            requestBuilder
                .AddQueryParam("vote_count.gte", minVoteCount)
                .AddQueryParam("vote_average.gte", minVoteAverage)
                .AddQueryParam("with_genres", includeGenreIds)
                .AddQueryParam("without_genres", excludeGenreIds)
                .AddQueryParam("with_original_language", languageCode)
                .Accept(HttpAccept.Json);

            for (var pageNumber = 1; pageNumber <= MaxPages; pageNumber++)
            {
                Logger.Info($"Importing TMDb movies from: {requestBuilder.BaseUrl}&page={pageNumber}");

                requestBuilder.AddQueryParam("page", pageNumber, true);

                yield return new NetImportRequest(requestBuilder.Build());
            }
        }
    }
}
