using Newtonsoft.Json;
using NzbDrone.Core.NetImport.Exceptions;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.NetImport.Kitsu
{
    public class KitsuParser : IParseNetImportResponse
    {
        private readonly KitsuSettings _settings;
        private NetImportResponse _importResponse;
        private readonly Logger _logger;

        public KitsuParser(KitsuSettings settings)
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

            var jsonResponse = JsonConvert.DeserializeObject<KitsuResponse>(_importResponse.Content);

            if (jsonResponse.included == null)
            {
                return movies;
            }

            foreach (var movie in jsonResponse.included)
            {
                Console.Write(movie);

                if (movie.attributes.subtype != "movie")
                {
                    continue;
                }

                movies.AddIfNotNull(new Tv.Movie()
                {
                    Title = movie.attributes.canonicalTitle,
                    // ImdbId = movie.movie.ids.imdb,
                    ImdbId = "tt0156887",
                    // TmdbId = movie.movie.ids.tmdb,
                    TmdbId = 10494,
                    Year = Int32.Parse(movie.attributes.startDate.Substring(0, 4))
                });
            }

            Console.Write(movies);

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
