using Newtonsoft.Json;
using NzbDrone.Core.NetImport.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.NetImport.CouchPotato
{
    public class CouchPotatoParser : IParseNetImportResponse
    {
        private readonly CouchPotatoSettings _settings;
        private NetImportResponse _importResponse;
        private readonly Logger _logger;

        public CouchPotatoParser(CouchPotatoSettings settings)
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
                        movies.AddIfNotNull(new Movies.Movie()
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
                            movies.AddIfNotNull(new Movies.Movie()
                            {
                                Title = item.title,
                                ImdbId = item.info.imdb,
                                TmdbId = tmdbid,
                                Monitored = false
                            });
                        }
                    }
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
