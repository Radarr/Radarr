using System.Collections.Generic;
using NzbDrone.Api.Movies;
using Radarr.Http.REST;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Api.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public MovieResource Movie { get; set; }
    }
}
