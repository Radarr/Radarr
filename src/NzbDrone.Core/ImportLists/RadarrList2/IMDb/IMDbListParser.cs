using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;

namespace NzbDrone.Core.ImportLists.RadarrList2.IMDbList
{
    public class IMDbListParser : RadarrList2Parser
    {
        private readonly IMDbListSettings _settings;

        public IMDbListParser(IMDbListSettings settings)
            : base()
        {
            _settings = settings;
        }

        public override IList<ImportListMovie> ParseResponse(ImportListResponse importListResponse)
        {
            var importResponse = importListResponse;

            var movies = new List<ImportListMovie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            if (_settings.ListId.StartsWith("ls", StringComparison.OrdinalIgnoreCase))
            {
                //Parse TSV response from IMDB export
                var row = importResponse.Content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                movies = row.Skip(1).SelectList(m => new ImportListMovie { ImdbId = m.Split(',')[1] });

                return movies;
            }
            else
            {
                var jsonResponse = JsonConvert.DeserializeObject<List<MovieResource>>(importResponse.Content);

                if (jsonResponse == null)
                {
                    return movies;
                }

                return jsonResponse.SelectList(m => new ImportListMovie { TmdbId = m.TmdbId });
            }
        }
    }
}
