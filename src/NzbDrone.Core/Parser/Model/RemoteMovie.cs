using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteMovie
    {
        public ReleaseInfo Release { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; } //TODO: Change to ParsedMovieInfo, for now though ParsedEpisodeInfo will do.
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public Movie Movie { get; set; }
        public bool DownloadAllowed { get; set; }

        public override string ToString()
        {
            return Release.Title;
        }
    }
}