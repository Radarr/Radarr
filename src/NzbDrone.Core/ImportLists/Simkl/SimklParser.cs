using System.Collections.Generic;
using System.Net;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListMovies;

namespace NzbDrone.Core.ImportLists.Simkl
{
    public class SimklParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;

        public SimklParser()
        {
        }

        public virtual IList<ImportListMovie> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var movie = new List<ImportListMovie>();

            if (!PreProcess(_importResponse))
            {
                return movie;
            }

            var jsonResponse = STJson.Deserialize<SimklResponse>(_importResponse.Content);

            // no shows were return
            if (jsonResponse == null)
            {
                return movie;
            }

            foreach (var show in jsonResponse.Movies)
            {
                movie.AddIfNotNull(new ImportListMovie()
                {
                    Title = show.Movie.Title,
                    TmdbId = int.TryParse(show.Movie.Ids.Tmdb, out var tmdbId) ? tmdbId : 0,
                    ImdbId = show.Movie.Ids.Imdb
                });
            }

            return movie;
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
