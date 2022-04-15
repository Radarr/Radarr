using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class UpgradeSpecification : IImportDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public UpgradeSpecification(IConfigService configService,
                                    ICustomFormatCalculationService formatService,
                                    Logger logger)
        {
            _configService = configService;
            _formatService = formatService;
            _logger = logger;
        }

        public IEnumerable<Decision> IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var profiles = localMovie.Movie.QualityProfiles.Value;
            var files = localMovie.Movie.MovieFiles.Value;
            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;

            if (files.Count == 0)
            {
                yield return Decision.Accept();
                yield break;
            }

            foreach (var file in files)
            {
                file.Movie = localMovie.Movie;
                var currentFormats = _formatService.ParseCustomFormat(file);

                foreach (var profile in profiles)
                {
                    yield return Calculate(profile, localMovie, file, currentFormats, downloadPropersAndRepacks);
                }
            }
        }

        private Decision Calculate(QualityProfile profile, LocalMovie localMovie, MovieFile file, List<CustomFormat> currentFormats, ProperDownloadTypes downloadPropersAndRepacks)
        {
            var qualityComparer = new QualityModelComparer(profile);
            var profileId = profile.Id;

            // Check to see if the existing file is valid for this profile. if not, don't count against this release
            var qualityIndex = profile.GetIndex(file.Quality.Quality);
            var qualityOrGroup = profile.Items[qualityIndex.Index];

            if (!qualityOrGroup.Allowed)
            {
                return Decision.Reject("Quality not wanted in Profile", profileId);
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
                return Decision.Reject($"Not an upgrade for existing movie file. New Quality is {localMovie.Quality.Quality}", profileId);
            }

            // Same quality, propers/repacks are preferred and it is not a revision update. Reject revision downgrade.

            if (qualityCompare == 0 &&
                downloadPropersAndRepacks != ProperDownloadTypes.DoNotPrefer &&
                localMovie.Quality.Revision.CompareTo(movieFile.Quality.Revision) < 0)
            {
                _logger.Debug("This file isn't a quality revision upgrade for movie. Skipping {0}", localMovie.Path);
                return Decision.Reject("Not a quality revision upgrade for existing movie file(s)", profileId);
            }

            var currentScore = profile.CalculateCustomFormatScore(currentFormats);

            if (qualityCompare == 0 && localMovie.CustomFormatScore < currentScore)
            {
                _logger.Debug("New file's custom formats [{0}] do not improve on [{1}], skipping",
                    localMovie.CustomFormats.ConcatToString(),
                    currentFormats.ConcatToString());

                return Decision.Reject("Not a Custom Format upgrade for existing movie file(s)", profileId);
            }

            return Decision.Accept();
        }
    }
}
