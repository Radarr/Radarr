using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly LanguageUpgradableSpecification _languageUpgradableSpecification;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(QualityUpgradableSpecification qualityUpgradableSpecification, LanguageUpgradableSpecification languageUpgradableSpecification, Logger logger)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _languageUpgradableSpecification = languageUpgradableSpecification;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                _logger.Debug("Comparing file quality with report. Existing file is {0}", file.Quality);

                if (!_qualityUpgradableSpecification.IsUpgradable(subject.Series.Profile, file.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    return Decision.Reject("Quality for existing file on disk is of equal or higher preference: {0}", file.Quality);
                }
            }

            return Decision.Accept();
        }

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Movie.MovieFile.Value == null)
            {
                return Decision.Accept();
            }

            var file = subject.Movie.MovieFile.Value;

            // TODO :: 
            Parser.Language currentLanguage;
            if (file.MediaInfo.AudioLanguages.Contains("Hungarian"))
            {
                currentLanguage = Parser.Language.Hungarian;
            }
            else
            {
                currentLanguage = Parser.Language.English;
            }

            if (!_languageUpgradableSpecification.IsUpgradable(subject.Movie.Profile, currentLanguage, subject.ParsedMovieInfo.Language))
            {
                    return Decision.Reject("Language for existing file on disk is of equal or higher preference: {0}", file.MediaInfo.AudioLanguages);
            }

            _logger.Debug("Comparing file quality with report. Existing file is {0}", file.Quality);

            if (!_qualityUpgradableSpecification.IsUpgradable(subject.Movie.Profile, file.Quality, subject.ParsedMovieInfo.Quality))
            {
                return Decision.Reject("Quality for existing file on disk is of equal or higher preference: {0}", file.Quality);
            }


            return Decision.Accept();
        }
    }
}
