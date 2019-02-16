using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
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

        public Decision IsSatisfiedBy(LocalAlbumRelease localAlbumRelease)
        {
            var existingRelease = localAlbumRelease.AlbumRelease.Album.Value.AlbumReleases.Value.Single(x => x.Monitored);
            if (localAlbumRelease.AlbumRelease.Id != existingRelease.Id &&
                localAlbumRelease.TrackCount < existingRelease.Tracks.Value.Count(x => x.HasFile))
            {
                _logger.Debug("This release has fewer tracks than the existing one. Skipping {0}", localAlbumRelease);
                return Decision.Reject("Has fewer tracks than existing release");
            }

            _logger.Trace("Accepting release {0}", localAlbumRelease);
            return Decision.Accept();
        }
    }
}
