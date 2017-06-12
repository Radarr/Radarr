﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nancy.Responses;
using NLog;
using Nancy;
using NzbDrone.Api.Frontend.Mappers;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Frontend
{
    public class StaticResourceModule : NancyModule
    {
        private readonly IEnumerable<IMapHttpRequestsToDisk> _requestMappers;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;


        public StaticResourceModule(IEnumerable<IMapHttpRequestsToDisk> requestMappers, IConfigFileProvider configFileProvider, Logger logger)
        {
            _requestMappers = requestMappers;
            _configFileProvider = configFileProvider;
            _logger = logger;

            Get["/{resource*}"] = x => Index();
            Get["/"] = x => Index();
        }

        private Response Index()
        {
            var path = Request.Url.Path;

            if (
                string.IsNullOrWhiteSpace(path) ||
                path.StartsWith("/api", StringComparison.CurrentCultureIgnoreCase) ||
                path.StartsWith("/signalr", StringComparison.CurrentCultureIgnoreCase))
            {
                return new NotFoundResponse();
            }

            var mapper = _requestMappers.SingleOrDefault(m => m.CanHandle(path));

            if (mapper != null)
            {
                return mapper.GetResponse(path);
            }

            _logger.Warn("Couldn't find handler for {0}", path);

            return new NotFoundResponse();
        }
    }
}
