using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Trakt.Resource;

namespace NzbDrone.Core.NetImport.Trakt.Popular
{
    public class TraktPopularParser : TraktParser
    {
        private readonly TraktPopularSettings _settings;
        private NetImportResponse _importResponse;

        public TraktPopularParser(TraktPopularSettings settings)
        {
            _settings = settings;
        }

        public override IList<Movie> ParseResponse(NetImportResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<Movie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            var jsonResponse = new List<TraktMovieResource>();

            if (_settings.TraktListType == (int)TraktPopularListType.Popular)
            {
                jsonResponse = JsonConvert.DeserializeObject<List<TraktMovieResource>>(_importResponse.Content);
            }
            else
            {
                jsonResponse = JsonConvert.DeserializeObject<List<TraktListResource>>(_importResponse.Content).SelectList(c => c.Movie);
            }

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            foreach (var movie in jsonResponse)
            {
                movies.AddIfNotNull(new Movies.Movie()
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
