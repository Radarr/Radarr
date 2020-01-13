using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.RadarrAPI;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.NetImport.RadarrList
{
    public class RadarrListParser : IParseNetImportResponse
    {
        private readonly ISearchForNewMovie _skyhookProxy;

        public RadarrListParser(ISearchForNewMovie skyhookProxy)
        {
            _skyhookProxy = skyhookProxy;
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

            return jsonResponse.SelectList(_skyhookProxy.MapMovie);
        }

        protected virtual bool PreProcess(NetImportResponse netImportResponse)
        {
            try
            {
                var error = JsonConvert.DeserializeObject<RadarrError>(netImportResponse.HttpResponse.Content);

                if (error != null && error.Errors != null && error.Errors.Count != 0)
                {
                    throw new RadarrAPIException(error);
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
