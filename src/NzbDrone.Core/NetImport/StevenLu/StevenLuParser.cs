using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.NetImport.Exceptions;
using NzbDrone.Core.NetImport.ListMovies;

namespace NzbDrone.Core.NetImport.StevenLu
{
    public class StevenLuParser : IParseNetImportResponse
    {
        private NetImportResponse _importResponse;

        public StevenLuParser()
        {
        }

        public IList<ListMovie> ParseResponse(NetImportResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<ListMovie>();

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
                movies.AddIfNotNull(new ListMovie()
                {
                    Title = item.title,
                    ImdbId = item.imdb_id,
                });
            }

            return movies;
        }

        protected virtual bool PreProcess(NetImportResponse netImportResponse)
        {
            if (netImportResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new NetImportException(netImportResponse, "StevenLu API call resulted in an unexpected StatusCode [{0}]", netImportResponse.HttpResponse.StatusCode);
            }

            if (netImportResponse.HttpResponse.Headers.ContentType != null && netImportResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                netImportResponse.HttpRequest.Headers.Accept != null && !netImportResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new NetImportException(netImportResponse, "StevenLu responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
