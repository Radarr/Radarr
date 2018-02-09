using NzbDrone.Api.Episodes;
using NzbDrone.Api.Series;
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
            return null;
        }
    }
}