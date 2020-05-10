using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.NetImport.TMDb.List
{
    public class TMDbListParser : TMDbParser
    {
        private readonly ISearchForNewMovie _skyhookProxy;

        public TMDbListParser(ISearchForNewMovie skyhookProxy)
            : base(skyhookProxy)
        {
            _skyhookProxy = skyhookProxy;
        }

        public override IList<Movie> ParseResponse(NetImportResponse importResponse)
        {
            var movies = new List<Movie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<ListResponseRoot>(importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            foreach (var movie in jsonResponse.items)
            {
                // Movies with no Year Fix
                if (string.IsNullOrWhiteSpace(movie.release_date))
                {
                    continue;
                }

                movies.AddIfNotNull(new Movie { TmdbId = movie.id });
            }

            return movies;
        }
    }
}
