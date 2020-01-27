using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.RadarrAPI;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;

namespace NzbDrone.Core.NetImport.Radarr
{
    public class RadarrParser : IParseNetImportResponse
    {
        private readonly RadarrSettings _settings;
        private readonly ISearchForNewMovie _skyhookProxy;
        private NetImportResponse _importResponse;

        public RadarrParser(RadarrSettings settings, ISearchForNewMovie skyhookProxy)
        {
            _skyhookProxy = skyhookProxy;
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

            var jsonResponse = JsonConvert.DeserializeObject<List<MovieResult>>(_importResponse.Content);

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
