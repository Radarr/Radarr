using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Aggregation;
using NzbDrone.Core.Parser;
using Radarr.Api.V3.Movies;
using Radarr.Http;

namespace Radarr.Api.V3.Parse
{
    [V3ApiController]
    public class ParseController : Controller
    {
        private readonly IParsingService _parsingService;
        private readonly IConfigService _configService;
        private readonly IRemoteMovieAggregationService _aggregationService;

        public ParseController(IParsingService parsingService,
                               IConfigService configService,
                               IRemoteMovieAggregationService aggregationService)
        {
            _parsingService = parsingService;
            _configService = configService;
            _aggregationService = aggregationService;
        }

        [HttpGet]
        public ParseResource Parse(string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return null;
            }

            var parsedMovieInfo = Parser.ParseMovieTitle(title);

            if (parsedMovieInfo == null)
            {
                return new ParseResource
                {
                    Title = title
                };
            }

            var remoteMovie = _parsingService.Map(parsedMovieInfo, "", 0);

            _aggregationService.Augment(remoteMovie);

            if (remoteMovie != null)
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedMovieInfo = remoteMovie.ParsedMovieInfo,
                    Movie = remoteMovie.Movie.ToResource(_configService.AvailabilityDelay)
                };
            }
            else
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedMovieInfo = parsedMovieInfo
                };
            }
        }
    }
}
