using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListMovies;

namespace NzbDrone.Core.ImportLists.Simkl
{
    public class SimklParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(SimklParser));

        public virtual IList<ImportListMovie> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<ImportListMovie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            var jsonResponse = Json.Deserialize<SimklResponse>(_importResponse.Content);

            // no movies were returned
            if (jsonResponse == null)
            {
                return movies;
            }

            if (jsonResponse.Anime != null)
            {
                foreach (var movie in jsonResponse.Anime)
                {
                    if (int.TryParse(movie.Show.Ids.Tmdb, out var tmdbId) && tmdbId > 0 && movie.AnimeType is SimklAnimeType.Movie)
                    {
                        movies.AddIfNotNull(new ImportListMovie
                        {
                            Title = movie.Show.Title,
                            ImdbId = movie.Show.Ids.Imdb,
                            TmdbId = tmdbId,
                        });
                    }
                    else
                    {
                        Logger.Warn("Skipping info grabbing for '{0}' because it is an unsupported content type.", movie.Show.Title);
                    }
                }
            }

            if (jsonResponse.Movies != null)
            {
                foreach (var movie in jsonResponse.Movies)
                {
                    movies.AddIfNotNull(new ImportListMovie
                    {
                        Title = movie.Movie.Title,
                        TmdbId = int.TryParse(movie.Movie.Ids.Tmdb, out var tmdbId) ? tmdbId : 0,
                        ImdbId = movie.Movie.Ids.Imdb,
                    });
                }
            }

            return movies;
        }

        protected virtual bool PreProcess(ImportListResponse netImportResponse)
        {
            if (netImportResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(netImportResponse, "Simkl API call resulted in an unexpected StatusCode [{0}]", netImportResponse.HttpResponse.StatusCode);
            }

            if (netImportResponse.HttpResponse.Headers.ContentType != null && netImportResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                netImportResponse.HttpRequest.Headers.Accept != null && !netImportResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(netImportResponse, "Simkl API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
