using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class SameFileSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SameFileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var movieFile = localMovie.Movie.MovieFile;

            if (localMovie.Movie.MovieFileId == 0)
            {
                _logger.Debug("No existing movie file, skipping");
                return Decision.Accept();
            }

            if (movieFile == null)
            {
                var movie = localMovie.Movie;
                _logger.Trace("Unable to get movie file details from the DB. MovieId: {0} MovieFileId: {1}", movie.Id, movie.MovieFileId);

                return Decision.Accept();
            }

            if (movieFile.Size == localMovie.Size)
            {
                _logger.Debug("'{0}' Has the same filesize as existing file", localMovie.Path);
                return Decision.Reject("Has the same filesize as existing file");
            }

            return Decision.Accept();
        }
    }
}
