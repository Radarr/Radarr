using Newtonsoft.Json;
using NzbDrone.Core.NetImport.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.MetadataSource;

namespace NzbDrone.Core.NetImport.Radarr
{
    public class RadarrParser : IParseNetImportResponse
    {
        private readonly RadarrSettings _settings;
        private NetImportResponse _importResponse;
        private readonly ISearchForNewMovie _skyhookProxy;
        private readonly Logger _logger;

        public RadarrParser(RadarrSettings settings, ISearchForNewMovie skyhookProxy)
        {
            _skyhookProxy = skyhookProxy;//TinyIoC.TinyIoCContainer.Current.Resolve<ISearchForNewMovie>();
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

            var jsonResponse = JsonConvert.DeserializeObject<List<MovieResult>>(_importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            return jsonResponse.SelectList(_skyhookProxy.MapMovie);

            
        }

        protected virtual bool PreProcess(NetImportResponse indexerResponse)
        {
            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new NetImportException(indexerResponse, "List API call resulted in an unexpected StatusCode [{0}]", indexerResponse.HttpResponse.StatusCode);
            }

            if (indexerResponse.HttpResponse.Headers.ContentType != null && indexerResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                indexerResponse.HttpRequest.Headers.Accept != null && !indexerResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new NetImportException(indexerResponse, "List responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }

    }
}