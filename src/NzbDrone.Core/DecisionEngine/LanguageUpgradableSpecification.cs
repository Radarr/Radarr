using NLog;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine
{
    public interface ILanguageUpgradableSpecification
    {
        bool IsUpgradable(Profile profile, Parser.Language currentLanguage, Parser.Language newLanguage);
        bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality);
    }

    public class LanguageUpgradableSpecification : ILanguageUpgradableSpecification
    {
        private readonly Logger _logger;

        public LanguageUpgradableSpecification(Logger logger)
        {
            _logger = logger;
        }

        public bool IsUpgradable(Profile profile, Parser.Language currentLanguage, Parser.Language newLanguage)
        {
            if (currentLanguage == Parser.Language.Hungarian && newLanguage != Parser.Language.Hungarian)
            {
                return false;
            }

            return true;
        }

        public bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality)
        {
            var compare = newQuality.Revision.CompareTo(currentQuality.Revision);

            if (currentQuality.Quality == newQuality.Quality && compare > 0)
            {
                return true;
            }

            return false;
        }
    }
}
