using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.NetImport.Exceptions;
using NzbDrone.Core.NetImport.ListMovies;

namespace NzbDrone.Core.NetImport.RadarrList2
{
    public class RadarrList2Parser : IParseNetImportResponse
    {
        public IList<ListMovie> ParseResponse(NetImportResponse netMovieImporterResponse)
        {
            var importResponse = netMovieImporterResponse;

            var movies = new List<ListMovie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<List<MovieResource>>(importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            return jsonResponse.SelectList(m => new ListMovie { TmdbId = m.TmdbId });
        }

        protected virtual bool PreProcess(NetImportResponse listResponse)
        {
            if (listResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new NetImportException(listResponse,
                    "Radarr API call resulted in an unexpected StatusCode [{0}]",
                    listResponse.HttpResponse.StatusCode);
            }

            if (listResponse.HttpResponse.Headers.ContentType != null &&
                listResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                listResponse.HttpRequest.Headers.Accept != null &&
                !listResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new NetImportException(listResponse,
                    "Radarr API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
