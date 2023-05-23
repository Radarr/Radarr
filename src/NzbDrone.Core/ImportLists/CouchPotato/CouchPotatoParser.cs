using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListMovies;

namespace NzbDrone.Core.ImportLists.CouchPotato
{
    public class CouchPotatoParser : IParseImportListResponse
    {
        private ImportListResponse _importListResponse;

        public IList<ImportListMovie> ParseResponse(ImportListResponse importListResponse)
        {
            _importListResponse = importListResponse;

            var movies = new List<ImportListMovie>();

            if (!PreProcess(_importListResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<CouchPotatoResponse>(_importListResponse.Content);

            // no movies were return
            if (jsonResponse.total == 0)
            {
                return movies;
            }

            var responseData = jsonResponse.movies;

            foreach (var item in responseData)
            {
                var tmdbid = item.info?.tmdb_id ?? 0;

                // Fix weird error reported by Madmanali93
                if (item.type != null && item.releases != null)
                {
                    // if there are no releases at all the movie wasn't found on CP, so return movies
                    if (!item.releases.Any() && item.type == "movie")
                    {
                        movies.AddIfNotNull(new ImportListMovie()
                        {
                            Title = item.title,
                            ImdbId = item.info.imdb,
                            TmdbId = tmdbid
                        });
                    }
                    else
                    {
                        // snatched,missing,available,downloaded
                        // done,seeding
                        var isCompleted = item.releases.Any(rel => (rel.status == "done" || rel.status == "seeding"));
                        if (!isCompleted)
                        {
                            movies.AddIfNotNull(new ImportListMovie()
                            {
                                Title = item.title,
                                ImdbId = item.info.imdb,
                                TmdbId = tmdbid
                            });
                        }
                    }
                }
            }

            return movies;
        }

        protected virtual bool PreProcess(ImportListResponse importListResponse)
        {
            if (importListResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(importListResponse, "List API call resulted in an unexpected StatusCode [{0}]", importListResponse.HttpResponse.StatusCode);
            }

            if (importListResponse.HttpResponse.Headers.ContentType != null && importListResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                importListResponse.HttpRequest.Headers.Accept != null && !importListResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(importListResponse, "List responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
