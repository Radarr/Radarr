using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class NotSampleSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDetectSample _detectSample;
        private readonly Logger _logger;

        public NotSampleSpecification(IDetectSample detectSample,
                                      Logger logger)
        {
            _detectSample = detectSample;
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.ExistingFile)
            {
                _logger.Debug("Existing file, skipping sample check");
                return Decision.Accept();
            }

            var sample = _detectSample.IsSample(localMovie.Movie.MovieMetadata, localMovie.Path);

            if (sample == DetectSampleResult.Sample)
            {
                return Decision.Reject("Sample");
            }
            else if (sample == DetectSampleResult.Indeterminate)
            {
                return Decision.Reject("Unable to determine if file is a sample");
            }

            return Decision.Accept();
        }
    }
}
