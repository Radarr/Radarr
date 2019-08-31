using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public interface IUpgradableSpecification
    {
        bool IsUpgradable(Profile profile, QualityModel currentQuality, QualityModel newQuality = null);
        bool CutoffNotMet(Profile profile, QualityModel currentQuality, QualityModel newQuality = null);
        bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality);
        bool IsUpgradeAllowed(Profile qualityProfile, QualityModel currentQuality, QualityModel newQuality);
    }

    public class UpgradableSpecification : IUpgradableSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public UpgradableSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public bool IsUpgradable(Profile profile, QualityModel currentQuality, QualityModel newQuality = null)
        {
            if (newQuality != null)
            {
                int compare = new QualityModelComparer(profile).Compare(newQuality, currentQuality);
                if (compare <= 0)
                {
                    return false;
                }

                if (IsRevisionUpgrade(currentQuality, newQuality))
                {
                    _logger.Debug("New item has a better quality revision");
                    return true;
                }
            }

            _logger.Debug("New item has a better quality");
            return true;
        }

        public bool CutoffNotMet(Profile profile, QualityModel currentQuality, QualityModel newQuality = null)
        {
            var comparer = new QualityModelComparer(profile);
            var cutoffCompare = comparer.Compare(currentQuality.Quality.Id, profile.Cutoff);

            if (cutoffCompare < 0)
            {
                return true;
            }

            if (comparer.Compare(currentQuality.CustomFormats, profile.FormatCutoff) < 0)
            {
                return true;
            }

            if (newQuality != null && IsRevisionUpgrade(currentQuality, newQuality))
            {
                return true;
            }

            return false;

        }

        public bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality)
        {
            var compare = newQuality.Revision.CompareTo(currentQuality.Revision);

            if (currentQuality.Quality == newQuality.Quality && compare > 0)
            {
                _logger.Debug("New quality is a better revision for existing quality");
                return true;
            }

            return false;
        }

        public bool IsUpgradeAllowed(Profile qualityProfile, QualityModel currentQuality, QualityModel newQuality)
        {
            var isQualityUpgrade = new QualityModelComparer(qualityProfile).Compare(newQuality, currentQuality) > 0;

            if (isQualityUpgrade && qualityProfile.UpgradeAllowed)
            {
                _logger.Debug("Quality profile allows upgrading");
                return true;
            }

            return false;
        }
    }
}
