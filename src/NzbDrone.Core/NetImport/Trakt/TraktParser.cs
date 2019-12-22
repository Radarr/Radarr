using Newtonsoft.Json;
using NzbDrone.Core.NetImport.Exceptions;
using System.Collections.Generic;
using System.Net;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.NetImport.Trakt
{
    public class TraktParser : IParseNetImportResponse
    {
        private readonly TraktSettings _settings;
        private NetImportResponse _importResponse;

        public TraktParser(TraktSettings settings)
        {
            _settings = settings;
        }

        public IList<Movies.Movie> ParseResponse(NetImportResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<Movies.Movie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            if (_settings.ListType == (int) TraktListType.Popular)
            {
                var jsonResponse = JsonConvert.DeserializeObject<List<Movie>>(_importResponse.Content);

                foreach (var movie in jsonResponse)
                {
                    movies.AddIfNotNull(new Movies.Movie()
                    {
                        Title = movie.title,
                        ImdbId = movie.ids.imdb,
                        TmdbId = movie.ids.tmdb,
                        Year = (movie.year ?? 0)
                    });
                }
            }
            else
            {
                var jsonResponse = JsonConvert.DeserializeObject<List<TraktResponse>>(_importResponse.Content);

                // no movies were return
                if (jsonResponse == null)
                {
                    return movies;
                }

                foreach (var movie in jsonResponse)
                {
                    movies.AddIfNotNull(new Movies.Movie()
                    {
                        Title = movie.movie.title,
                        ImdbId = movie.movie.ids.imdb,
                        TmdbId = movie.movie.ids.tmdb,
                        Year = (movie.movie.year ?? 0)
                    });
                }
            }

            return movies;

        }

        protected virtual bool PreProcess(NetImportResponse indexerResponse)
        {
            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new NetImportException(indexerResponse, "Indexer API call resulted in an unexpected StatusCode [{0}]", indexerResponse.HttpResponse.StatusCode);
            }

            if (indexerResponse.HttpResponse.Headers.ContentType != null && indexerResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                indexerResponse.HttpRequest.Headers.Accept != null && !indexerResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new NetImportException(indexerResponse, "Indexer responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }

    }
}
