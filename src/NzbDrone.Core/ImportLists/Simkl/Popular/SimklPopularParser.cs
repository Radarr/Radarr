using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Notifications.Simkl.Resource;

namespace NzbDrone.Core.ImportLists.Simkl.Popular
{
    public class SimklPopularParser : SimklParser
    {
        private readonly SimklPopularSettings _settings;
        private ImportListResponse _importResponse;

        public SimklPopularParser(SimklPopularSettings settings)
        {
            _settings = settings;
        }

        public override IList<ImportListMovie> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<ImportListMovie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            var jsonResponse = new List<SimklMovieResource>();

            if (_settings.SimklListType == (int)SimklPopularListType.Popular)
            {
                jsonResponse = JsonConvert.DeserializeObject<List<SimklMovieResource>>(_importResponse.Content);
            }
            else
            {
                jsonResponse = JsonConvert.DeserializeObject<List<SimklListResource>>(_importResponse.Content).SelectList(c => c.Movie);
            }

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            foreach (var movie in jsonResponse)
            {
                movies.AddIfNotNull(new ImportListMovie()
                {
                    Title = movie.Title,
                    ImdbId = movie.Ids.Imdb,
                    TmdbId = movie.Ids.Tmdb,
                    Year = movie.Year ?? 0
                });
            }

            return movies;
        }
    }
}
