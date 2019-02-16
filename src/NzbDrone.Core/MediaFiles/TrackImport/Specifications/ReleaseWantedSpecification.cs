using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Specifications
{
    public class ReleaseWantedSpecification : IImportDecisionEngineSpecification<LocalAlbumRelease>
    {
        private readonly Logger _logger;

        public ReleaseWantedSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalAlbumRelease localAlbumRelease)
        {
            if (localAlbumRelease.AlbumRelease.Monitored || localAlbumRelease.AlbumRelease.Album.Value.AnyReleaseOk)
            {
                return Decision.Accept();
            }

            _logger.Debug("AlbumRelease {0} was not requested", localAlbumRelease);
            return Decision.Reject("Album release not requested");
        }
    }
}
