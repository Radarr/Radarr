using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class HasAudioTrackSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public HasAudioTrackSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.MediaInfo == null)
            {
                _logger.Debug("Failed to get media info from the file, make sure ffprobe is available, skipping check");
                return Decision.Accept();
            }

            if (localMovie.MediaInfo.AudioStreamCount == 0)
            {
                _logger.Debug("No audio tracks found in file");

                return Decision.Reject("No audio tracks detected");
            }

            return Decision.Accept();
        }
    }
}
