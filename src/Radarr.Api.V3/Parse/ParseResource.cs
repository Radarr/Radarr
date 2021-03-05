using NzbDrone.Core.Parser.Model;
using Radarr.Api.V3.Movies;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public MovieResource Movie { get; set; }
    }
}
