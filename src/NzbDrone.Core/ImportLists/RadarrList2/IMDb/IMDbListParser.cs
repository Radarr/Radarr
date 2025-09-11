using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;

namespace NzbDrone.Core.ImportLists.RadarrList2.IMDbList
{
    public class IMDbListParser : RadarrList2Parser
    {
        private readonly IMDbListSettings _settings;
        private readonly Logger _logger;

        public IMDbListParser(IMDbListSettings settings, Logger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public override IList<ImportListMovie> ParseResponse(ImportListResponse importListResponse)
        {
            var importResponse = importListResponse;

            var movies = new List<ImportListMovie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<List<MovieResource>>(importResponse.Content);

            if (jsonResponse == null)
            {
                return movies;
            }

            movies = jsonResponse.SelectList(m => new ImportListMovie { TmdbId = m.TmdbId });

            _logger.Debug("IMDb List {0}: Found {1} movies", _settings.ListId, movies.Count);

            return movies;
        }
    }
}
