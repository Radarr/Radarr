using System.Collections.Generic;
using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
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

        public IEnumerable<Decision> IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var profiles = localMovie.Movie.QualityProfiles.Value;
            var files = localMovie.Movie.MovieFiles.Value;

            if (files.Count == 0)
            {
                yield return Decision.Accept();
                yield break;
            }

            foreach (var file in files)
            {
                file.Movie = localMovie.Movie;

                foreach (var profile in profiles)
                {
                    yield return Calculate(profile, localMovie, file);
                }
            }
        }

        private Decision Calculate(Profile profile, LocalMovie localMovie, MovieFile file)
        {
            var qualityComparer = new QualityModelComparer(profile);

            // Check to see if the existing file is valid for this profile. if not, don't count against this release
            var qualityIndex = profile.GetIndex(file.Quality.Quality);
            var qualityOrGroup = profile.Items[qualityIndex.Index];

            if (!qualityOrGroup.Allowed)
            {
                return Decision.Accept();
            }

            var movieFile = file;

            if (movieFile == null)
            {
                _logger.Trace("Unable to get movie file details from the DB. MovieId: {0} MovieFileId: {1}", localMovie.Movie.Id, file.Id);
                return Decision.Accept();
            }

            var qualityCompare = qualityComparer.Compare(localMovie.Quality.Quality, movieFile.Quality.Quality);

            if (qualityCompare < 0)
            {
                _logger.Debug("This file isn't a quality upgrade for movie. New Quality is {0}. Skipping {1}", localMovie.Quality.Quality, localMovie.Path);
                return Decision.Reject(string.Format("Not an upgrade for existing movie file. New Quality is {0}", localMovie.Quality.Quality));
            }

            return Decision.Accept();
        }
    }
}
