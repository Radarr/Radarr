using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class UpgradeSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public UpgradeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var qualityComparer = new QualityModelComparer(localMovie.Movie.Profile);
            if (localMovie.Movie.MovieFile != null && qualityComparer.Compare(localMovie.Movie.MovieFile.Quality, localMovie.Quality) > 0)
            {
                _logger.Debug("This file isn't an upgrade for movie. Skipping {0}", localMovie.Path);
                return Decision.Reject("Not an upgrade for existing movie file");
            }

            return Decision.Accept();
        }
    }
}
