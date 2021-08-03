using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Parser;
using Radarr.Api.V3.Movies;
using Radarr.Http;

namespace Radarr.Api.V3.Parse
{
    public class ParseModule : RadarrRestModule<ParseResource>
    {
        private readonly IParsingService _parsingService;
        private readonly IConfigService _configService;

        public ParseModule(IParsingService parsingService, IConfigService configService)
        {
            _parsingService = parsingService;
            _configService = configService;

            GetResourceSingle = Parse;
        }

        private ParseResource Parse()
        {
            var title = Request.Query.Title.Value as string;

            if (title.IsNullOrWhiteSpace())
            {
                return null;
            }

            var parsedMovieInfo = _parsingService.ParseMovieInfo(title, new List<object>());

            if (parsedMovieInfo == null)
            {
                return new ParseResource
                {
                    Title = title
                };
            }

            var remoteMovie = _parsingService.Map(parsedMovieInfo, "");

            if (remoteMovie != null)
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedMovieInfo = remoteMovie.RemoteMovie.ParsedMovieInfo,
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
