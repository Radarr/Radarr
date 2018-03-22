﻿using Newtonsoft.Json;
using NzbDrone.Core.NetImport.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.RadarrAPI;
using NzbDrone.Common.Http;

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

        protected virtual bool PreProcess(NetImportResponse indexerResponse)
        {
            try
            {
                var error = JsonConvert.DeserializeObject<RadarrError>(indexerResponse.HttpResponse.Content);

                if (error != null && error.Errors != null && error.Errors.Count != 0)
                {
                    throw new RadarrAPIException(error);
                }
            }
            catch (JsonSerializationException)
            {
                //No error!
            }


            if (indexerResponse.HttpResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpException(indexerResponse.HttpRequest, indexerResponse.HttpResponse);
            }

            return true;
        }

    }
}