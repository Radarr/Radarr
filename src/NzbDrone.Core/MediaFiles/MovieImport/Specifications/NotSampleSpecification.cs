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

        public Decision IsSatisfiedBy(LocalMovie localEpisode, DownloadClientItem downloadClientItem)
        {
            var sample = _detectSample.IsSample(localEpisode.Movie,
                                                localEpisode.Quality,
                                                localEpisode.Path,
                                                localEpisode.Size,
                                                false);

            if (sample)
            {
                return Decision.Reject("Sample");
            }

            return Decision.Accept();
        }
    }
}
