using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Specifications
{
    public class NoMissingOrUnmatchedTracksSpecification : IImportDecisionEngineSpecification<LocalAlbumRelease>
    {
        private readonly Logger _logger;

        public NoMissingOrUnmatchedTracksSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalAlbumRelease localAlbumRelease)
        {
            if (localAlbumRelease.NewDownload && localAlbumRelease.TrackMapping.LocalExtra.Count > 0)
            {
                _logger.Debug("This release has track files that have not been matched. Skipping {0}", localAlbumRelease);
                return Decision.Reject("Has unmatched tracks");
            }

            if (localAlbumRelease.NewDownload && localAlbumRelease.TrackMapping.MBExtra.Count > 0)
            {
                _logger.Debug("This release is missing tracks. Skipping {0}", localAlbumRelease);
                return Decision.Reject("Has missing tracks");
            }
            
            return Decision.Accept();
        }
    }
}
