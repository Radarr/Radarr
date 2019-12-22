using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteMovie
    {
        public ReleaseInfo Release { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public Movie Movie { get; set; }
        public MappingResultType MappingResult { get; set; }
        public bool DownloadAllowed { get; set; }
        public TorrentSeedConfiguration SeedConfiguration { get; set; }

        public override string ToString()
        {
            return Release.Title;
        }
    }
}
