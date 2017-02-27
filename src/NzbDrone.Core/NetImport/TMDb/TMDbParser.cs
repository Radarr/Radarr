using Newtonsoft.Json;
using NzbDrone.Core.NetImport.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;

namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbParser : IParseNetImportResponse
    {
        private readonly TMDbSettings _settings;
        private NetImportResponse _importResponse;
        private readonly Logger _logger;

        public TMDbParser(TMDbSettings settings)
        {
            _settings = settings;
        }

        public IList<Tv.Movie> ParseResponse(NetImportResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<Tv.Movie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            if (_settings.ListType != (int)TMDbListType.List)
            {
                var jsonResponse = JsonConvert.DeserializeObject<MovieSearchRoot>(_importResponse.Content);

                // no movies were return
                if (jsonResponse == null)
                {
                    return movies;
                }

                foreach (var movie in jsonResponse.results)
                {
                    // Movies with no Year Fix
                    if (string.IsNullOrWhiteSpace(movie.release_date))
                    {
                        continue;
                    }

                    movies.AddIfNotNull(new Tv.Movie()
                    {
                        Title = movie.title,
                        TmdbId = movie.id,
                        ImdbId = null,
                        Year = DateTime.Parse(movie.release_date).Year
                    });
                }
            }
            else
            {
                var jsonResponse = JsonConvert.DeserializeObject<ListResponseRoot>(_importResponse.Content);

                // no movies were return
                if (jsonResponse == null)
                {
                    return movies;
                }

                foreach (var movie in jsonResponse.items)
                {
                    // Skip non-movie things
                    if (movie.media_type != "movie")
                    {
                        continue;
                    }

                    // Movies with no Year Fix
                    if (string.IsNullOrWhiteSpace(movie.release_date))
                    {
                        continue;
                    }

                    movies.AddIfNotNull(new Tv.Movie()
                    {
                        Title = movie.title,
                        TmdbId = movie.id,
                        ImdbId = null,
                        Year = DateTime.Parse(movie.release_date).Year
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