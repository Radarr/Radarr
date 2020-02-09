using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Specifications
{
    public class MoreTracksSpecification : IImportDecisionEngineSpecification<LocalAlbumRelease>
    {
        private readonly Logger _logger;

        public MoreTracksSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalAlbumRelease item, DownloadClientItem downloadClientItem)
        {
            var existingRelease = item.AlbumRelease.Album.Value.AlbumReleases.Value.Single(x => x.Monitored);
            var existingTrackCount = existingRelease.Tracks.Value.Count(x => x.HasFile);
            if (item.AlbumRelease.Id != existingRelease.Id &&
                item.TrackCount < existingTrackCount)
            {
                _logger.Debug($"This release has fewer tracks ({item.TrackCount}) than existing {existingRelease} ({existingTrackCount}). Skipping {item}");
                return Decision.Reject("Has fewer tracks than existing release");
            }

            _logger.Trace("Accepting release {0}", item);
            return Decision.Accept();
        }
    }
}
