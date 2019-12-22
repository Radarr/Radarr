using NzbDrone.Api.Movies;
using NzbDrone.Core.Parser.Model;
using Radarr.Http.REST;

namespace NzbDrone.Api.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public MovieResource Movie { get; set; }
    }
}
