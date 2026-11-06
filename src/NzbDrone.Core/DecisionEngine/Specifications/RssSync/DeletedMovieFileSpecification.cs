using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class DeletedMovieFileSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DeletedMovieFileSpecification(IDiskProvider diskProvider, IConfigService configService, Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Disk;
        public RejectionType Type => RejectionType.Temporary;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteMovie subject, ReleaseDecisionInformation information)
        {
            if (!_configService.AutoUnmonitorPreviouslyDownloadedMovies)
            {
                return DownloadSpecDecision.Accept();
            }

            if (information.SearchCriteria != null)
            {
                _logger.Debug("Skipping deleted movie file check during search");
                return DownloadSpecDecision.Accept();
            }

            if (subject.Movie.MovieFileId != 0 && IsMovieFileMissing(subject.Movie, subject.Movie.MovieFile))
            {
                _logger.Trace("Movie file {0} is missing from disk.", subject.Movie.MovieFile.RelativePath);

                _logger.Debug("File for this movie exist in the database but not on disk, will be unmonitored on next diskscan. skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MovieNotMonitored, "Movie is not monitored");
            }

            return DownloadSpecDecision.Accept();
        }

        private bool IsMovieFileMissing(Movie movie, MovieFile movieFile)
        {
            var fullPath = Path.Combine(movie.Path, movieFile.RelativePath);

            return !_diskProvider.FileExists(fullPath);
        }
    }
}
