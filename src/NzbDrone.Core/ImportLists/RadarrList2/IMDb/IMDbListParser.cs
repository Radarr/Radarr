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
                _logger.Debug("IMDb List {0}: Found {1} movies", _settings.ListId, movies.Count);
                return movies;
            }

            if (_settings.ListId.StartsWith("ls", StringComparison.OrdinalIgnoreCase))
            {
                // Parse TSV response from IMDB export
                var rows = importResponse.Content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                movies = rows.Skip(1).SelectList(m => m.Split(',')).Where(m => m.Length > 5).SelectList(i => new ImportListMovie { ImdbId = i[1], Title = i[5] });
            }
            else
            {
                var jsonResponse = JsonConvert.DeserializeObject<List<MovieResource>>(importResponse.Content);

                if (jsonResponse != null)
                {
                    movies = jsonResponse.SelectList(m => new ImportListMovie { TmdbId = m.TmdbId });
                }
            }

            _logger.Debug("IMDb List {0}: Found {1} movies", _settings.ListId, movies.Count);
            return movies;
        }
    }
}
