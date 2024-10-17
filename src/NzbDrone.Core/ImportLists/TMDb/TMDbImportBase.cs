using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.TMDb
{
    public abstract class TMDbImportListBase<TSettings> : HttpImportListBase<TSettings>
        where TSettings : TMDbSettingsBase<TSettings>, new()
    {
        public override ImportListType ListType => ImportListType.TMDB;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(12);
        public override int PageSize => 20;

        public readonly IHttpRequestBuilderFactory _requestBuilder;

        private readonly ICached<object> _cacheOptions;

        protected TMDbImportListBase(IRadarrCloudRequestBuilder requestBuilder,
            IHttpClient httpClient,
            IImportListStatusService importListStatusService,
            IConfigService configService,
            IParsingService parsingService,
            ICacheManager cacheManager,
            Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            _requestBuilder = requestBuilder.TMDB;

            _cacheOptions = cacheManager.GetCache<object>(GetType());
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "getTmdbGenres")
            {
                var genres = _cacheOptions.Get("tmdb_genres", FetchTmdbGenres, TimeSpan.FromDays(1));

                return new
                {
                    options = genres
                };
            }

            if (action == "getTmdbLanguages")
            {
                var languages = _cacheOptions.Get("tmdb_languages", FetchTmdbLanguages, TimeSpan.FromDays(1));

                return new
                {
                    options = languages
                };
            }

            return new { };
        }

        private object FetchTmdbGenres()
        {
            var request = _requestBuilder.Create()
                .SetSegment("api", "3")
                .SetSegment("route", "genre")
                .SetSegment("id", "")
                .SetSegment("secondaryRoute", "movie/list")
                .Build();

            var genres = Json.Deserialize<GenresResponseResource>(_httpClient.Execute(request).Content);

            return genres.Genres
                .OrderBy(r => r.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(r => new
                {
                    Value = r.Id,
                    Name = r.Name,
                    Hint = $"{r.Id}"
                });
        }

        private object FetchTmdbLanguages()
        {
            var request = _requestBuilder.Create()
                .SetSegment("api", "3")
                .SetSegment("route", "configuration")
                .SetSegment("id", "")
                .SetSegment("secondaryRoute", "languages")
                .Build();

            var languages = Json.Deserialize<List<LanguageResource>>(_httpClient.Execute(request).Content);

            return languages
                .OrderBy(r => r.Language, StringComparer.InvariantCultureIgnoreCase)
                .Select(r => new
                {
                    Value = r.Language,
                    Name = r.Language,
                    Hint = r.EnglishName
                });
        }
    }
}
