using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class MatchesGrabSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MatchesGrabSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            var releaseInfo = localMovie.Release;

            if (releaseInfo == null || releaseInfo.MovieIds.Empty())
            {
                return ImportSpecDecision.Accept();
            }

            if (releaseInfo.MovieIds.All(o => o != localMovie.Movie.Id))
            {
                _logger.Debug("Unexpected movie(s) in file: {0}", localMovie.Movie.ToString());

                return ImportSpecDecision.Reject(ImportRejectionReason.MovieNotFoundInRelease, "Movie {0} was not found in the grabbed release: {1}", localMovie.Movie.ToString(), releaseInfo.Title);
            }

            return ImportSpecDecision.Accept();
        }
    }
}
