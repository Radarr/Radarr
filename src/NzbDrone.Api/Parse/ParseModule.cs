using NzbDrone.Api.Movies;
using NzbDrone.Core.Parser;

namespace NzbDrone.Api.Parse
{
    public class ParseModule : NzbDroneRestModule<ParseResource>
    {
        private readonly IParsingService _parsingService;

        public ParseModule(IParsingService parsingService)
        {
            _parsingService = parsingService;

            GetResourceSingle = Parse;
        }

        private ParseResource Parse()
        {
            var title = Request.Query.Title.Value as string;
            var parsedMovieInfo = Parser.ParseMovieTitle(title, false);

            if (parsedMovieInfo == null)
            {
                return null;
            }

            var remoteMovie = _parsingService.Map(parsedMovieInfo, "");

            if (remoteMovie != null)
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedMovieInfo = remoteMovie.RemoteMovie.ParsedMovieInfo,
                    Movie = remoteMovie.Movie.ToResource()
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
