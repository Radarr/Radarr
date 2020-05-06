using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Specifications
{
    public class SameFileSpecification : IImportDecisionEngineSpecification<LocalTrack>
    {
        private readonly Logger _logger;

        public SameFileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalTrack item, DownloadClientItem downloadClientItem)
        {
            var trackFiles = item.Album?.BookFiles?.Value;

            if (trackFiles == null || !trackFiles.Any())
            {
                _logger.Debug("No existing track file, skipping");
                return Decision.Accept();
            }

            foreach (var trackFile in trackFiles)
            {
                if (trackFile.Size == item.Size)
                {
                    _logger.Debug("'{0}' Has the same filesize as existing file", item.Path);
                    return Decision.Reject("Has the same filesize as existing file");
                }
            }

            return Decision.Accept();
        }
    }
}
