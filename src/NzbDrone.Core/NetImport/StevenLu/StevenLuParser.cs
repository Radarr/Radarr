using Newtonsoft.Json;
using NzbDrone.Core.NetImport.Exceptions;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource;

namespace NzbDrone.Core.NetImport.StevenLu
{
    public class StevenLuParser : IParseNetImportResponse
    {
        private readonly StevenLuSettings _settings;
        private NetImportResponse _importResponse;
        private readonly Logger _logger;
        private readonly IProvideMovieIdService _movieIdService;

        public StevenLuParser(StevenLuSettings settings, IProvideMovieIdService movieIdService)
        {
            _settings = settings;
            _movieIdService = movieIdService;
        }

        public IList<Tv.Movie> ParseResponse(NetImportResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<Tv.Movie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<List<StevenLuResponse>>(_importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            foreach (var item in jsonResponse)
            {
                movies.AddIfNotNull(new Tv.Movie()
                {
                    Title = item.title,
                    ImdbId = item.imdb_id,
                    TmdbId = _movieIdService.GetTmdbIdByImdbId(item.imdb_id)
            });
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
