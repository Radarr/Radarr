using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteMovie
    {
        public ReleaseInfo Release { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public Movie Movie { get; set; }
        public MappingResultType MappingResult { get; set; }

        public bool IsRecentMovie()
        {
            if (Movie.PhysicalRelease.HasValue)
            {
                return Movie.PhysicalRelease.Value >= DateTime.UtcNow.AddDays(-21);
            }

            return true;
        }

        public override string ToString()
        {
            return Release.Title;
        }
    }
}
