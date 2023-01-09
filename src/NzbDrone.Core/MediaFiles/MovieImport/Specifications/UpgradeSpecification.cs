using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class UpgradeSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public UpgradeSpecification(ICustomFormatCalculationService customFormatCalculationService, Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var qualityComparer = new QualityModelComparer(localMovie.Movie.Profile);

            if (localMovie.Movie.MovieFileId > 0)
            {
                var movieFile = localMovie.Movie.MovieFile;

                if (movieFile == null)
                {
                    _logger.Trace("Unable to get movie file details from the DB. MovieId: {0} MovieFileId: {1}", localMovie.Movie.Id, localMovie.Movie.MovieFileId);
                    return Decision.Accept();
                }

                var qualityCompare = qualityComparer.Compare(localMovie.Quality.Quality, movieFile.Quality.Quality);

                if (qualityCompare < 0)
                {
                    _logger.Debug("This file isn't a quality upgrade for movie. New Quality is {0}. Skipping {1}", localMovie.Quality.Quality, localMovie.Path);
                    return Decision.Reject("Not an upgrade for existing movie file. New Quality is {0}", localMovie.Quality.Quality);
                }
            }

            return Decision.Accept();
        }
    }
}
