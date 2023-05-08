using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download.Aggregation;
using NzbDrone.Core.Parser;
using Radarr.Api.V3.CustomFormats;
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
        private readonly ICustomFormatCalculationService _formatCalculator;

        public ParseController(IParsingService parsingService,
                               IConfigService configService,
                               IRemoteMovieAggregationService aggregationService,
                               ICustomFormatCalculationService formatCalculator)
        {
            _parsingService = parsingService;
            _configService = configService;
            _aggregationService = aggregationService;
            _formatCalculator = formatCalculator;
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

            if (remoteMovie != null)
            {
                _aggregationService.Augment(remoteMovie);

                remoteMovie.CustomFormats = _formatCalculator.ParseCustomFormat(remoteMovie, 0);
                remoteMovie.CustomFormatScore = remoteMovie.Movie?.Profile?.CalculateCustomFormatScore(remoteMovie.CustomFormats) ?? 0;

                return new ParseResource
                {
                    Title = title,
                    ParsedMovieInfo = remoteMovie.ParsedMovieInfo,
                    Movie = remoteMovie.Movie.ToResource(_configService.AvailabilityDelay),
                    Languages = remoteMovie.Languages,
                    CustomFormats = remoteMovie.CustomFormats?.ToResource(false),
                    CustomFormatScore = remoteMovie.CustomFormatScore
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
