using System.Collections.Generic;
using System.Net;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Notifications.Trakt.Resource;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public class TraktParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;

        public virtual IList<ImportListMovie> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<ImportListMovie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            var jsonResponse = STJson.Deserialize<List<TraktListResource>>(_importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            foreach (var movie in jsonResponse)
            {
                movies.AddIfNotNull(new ImportListMovie()
                {
                    Title = movie.Movie.Title,
                    ImdbId = movie.Movie.Ids.Imdb,
                    TmdbId = movie.Movie.Ids.Tmdb,
                    Year = movie.Movie.Year ?? 0
                });
            }

            return movies;
        }

        protected virtual bool PreProcess(ImportListResponse importListResponse)
        {
            if (importListResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(importListResponse, "Trakt API call resulted in an unexpected StatusCode [{0}]", importListResponse.HttpResponse.StatusCode);
            }

            if (importListResponse.HttpResponse.Headers.ContentType != null && importListResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                importListResponse.HttpRequest.Headers.Accept != null && !importListResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(importListResponse, "Trakt API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
