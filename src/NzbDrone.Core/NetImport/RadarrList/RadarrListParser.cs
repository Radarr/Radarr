using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport.TMDb;

namespace NzbDrone.Core.NetImport.RadarrList
{
    public class RadarrListParser : IParseNetImportResponse
    {
        public RadarrListParser()
        {
        }

        public IList<Movie> ParseResponse(NetImportResponse netMovieImporterResponse)
        {
            var importResponse = netMovieImporterResponse;

            var movies = new List<Movie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<List<MovieResult>>(importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            return jsonResponse.SelectList(m => new Movie { TmdbId = m.id });
        }

        protected virtual bool PreProcess(NetImportResponse netImportResponse)
        {
            try
            {
                var error = JsonConvert.DeserializeObject<RadarrErrors>(netImportResponse.HttpResponse.Content);

                if (error != null && error.Errors != null && error.Errors.Count != 0)
                {
                    throw new RadarrListException(error);
                }
            }
            catch (JsonSerializationException)
            {
                //No error!
            }

            if (netImportResponse.HttpResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpException(netImportResponse.HttpRequest, netImportResponse.HttpResponse);
            }

            return true;
        }
    }
}
