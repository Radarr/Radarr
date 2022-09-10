using NzbDrone.Core.Parser.Model;
using Radarr.Api.V4.Movies;
using Radarr.Http.REST;

namespace Radarr.Api.V4.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public MovieResource Movie { get; set; }
    }
}
