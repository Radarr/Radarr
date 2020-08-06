using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.NetImport.Exceptions;
using NzbDrone.Core.NetImport.ListMovies;

namespace NzbDrone.Core.NetImport.CouchPotato
{
    public class CouchPotatoParser : IParseNetImportResponse
    {
        private readonly CouchPotatoSettings _settings;
        private NetImportResponse _importResponse;

        public CouchPotatoParser(CouchPotatoSettings settings)
        {
            _settings = settings;
        }

        public IList<ListMovie> ParseResponse(NetImportResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<ListMovie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<CouchPotatoResponse>(_importResponse.Content);

            // no movies were return
            if (jsonResponse.total == 0)
            {
                return movies;
            }

            var responseData = jsonResponse.movies;

            foreach (var item in responseData)
            {
                int tmdbid = item.info?.tmdb_id ?? 0;

                // Fix weird error reported by Madmanali93
                if (item.type != null && item.releases != null)
                {
                    // if there are no releases at all the movie wasn't found on CP, so return movies
                    if (!item.releases.Any() && item.type == "movie")
                    {
                        movies.AddIfNotNull(new ListMovie()
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
                        bool isCompleted = item.releases.Any(rel => (rel.status == "done" || rel.status == "seeding"));
                        if (!isCompleted)
                        {
                            movies.AddIfNotNull(new ListMovie()
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

        protected virtual bool PreProcess(NetImportResponse netImportResponse)
        {
            if (netImportResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new NetImportException(netImportResponse, "List API call resulted in an unexpected StatusCode [{0}]", netImportResponse.HttpResponse.StatusCode);
            }

            if (netImportResponse.HttpResponse.Headers.ContentType != null && netImportResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                netImportResponse.HttpRequest.Headers.Accept != null && !netImportResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new NetImportException(netImportResponse, "List responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
