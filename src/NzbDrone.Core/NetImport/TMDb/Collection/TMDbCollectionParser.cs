using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.NetImport.TMDb.Collection
{
    public class TMDbCollectionParser : TMDbParser
    {
        public override IList<Movie> ParseResponse(NetImportResponse importResponse)
        {
            var movies = new List<Movie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<CollectionResponseRoot>(importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            foreach (var movie in jsonResponse.parts)
            {
                // Movies with no Year Fix
                if (string.IsNullOrWhiteSpace(movie.release_date))
                {
                    continue;
                }

                movies.AddIfNotNull(MapListMovie(movie));
            }

            return movies;
        }
    }
}
