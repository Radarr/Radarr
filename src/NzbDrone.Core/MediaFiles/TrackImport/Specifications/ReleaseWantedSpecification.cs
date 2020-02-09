using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
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

        public Decision IsSatisfiedBy(LocalAlbumRelease item, DownloadClientItem downloadClientItem)
        {
            if (item.AlbumRelease.Monitored || item.AlbumRelease.Album.Value.AnyReleaseOk)
            {
                return Decision.Accept();
            }

            _logger.Debug("AlbumRelease {0} was not requested", item);
            return Decision.Reject("Album release not requested");
        }
    }
}
