using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
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

        public IEnumerable<Decision> IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            return new List<Decision> { Calculate(localMovie, downloadClientItem) };
        }

        public Decision Calculate(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.ExistingFile)
            {
                return Decision.Accept();
            }

            var releaseInfo = localMovie.Release;

            if (releaseInfo == null || releaseInfo.MovieIds.Empty())
            {
                return Decision.Accept();
            }

            if (releaseInfo.MovieIds.All(o => o != localMovie.Movie.Id))
            {
                _logger.Debug("Unexpected movie(s) in file: {0}", localMovie.Movie.ToString());

                return Decision.Reject($"Movie {localMovie.Movie.ToString()} was not found in the grabbed release: {releaseInfo.Title}", 0);
            }

            return Decision.Accept();
        }
    }
}
