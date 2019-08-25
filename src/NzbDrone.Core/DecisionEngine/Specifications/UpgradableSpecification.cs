using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public interface IUpgradableSpecification
    {
        bool IsUpgradable(QualityProfile profile, List<QualityModel> currentQualities, int currentScore, QualityModel newQuality, int newScore);
        bool QualityCutoffNotMet(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null);
        bool CutoffNotMet(QualityProfile profile, List<QualityModel> currentQualities, int currentScore, QualityModel newQuality = null, int newScore = 0);
        bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality);
        bool IsUpgradeAllowed(QualityProfile qualityProfile, List<QualityModel> currentQualities, QualityModel newQuality);
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

        private ProfileComparisonResult IsQualityUpgradable(QualityProfile profile, List<QualityModel> currentQualities, QualityModel newQuality = null)
        {
            if (newQuality != null)
            {
                var totalCompare = 0;

                foreach (var quality in currentQualities)
                {
                    var compare = new QualityModelComparer(profile).Compare(newQuality, quality);

                    totalCompare += compare;

                    if (compare < 0)
                    {
                        // Not upgradable if new quality is a downgrade for any current quality
                        return ProfileComparisonResult.Downgrade;
                    }
                }

                // Not upgradable if new quality is equal to all current qualities
                if (totalCompare == 0) {
                    return ProfileComparisonResult.Equal;
                }

                // Quality Treated as Equal if Propers are not Prefered
                if (_configService.DownloadPropersAndRepacks == ProperDownloadTypes.DoNotPrefer &&
                    newQuality.Revision.CompareTo(currentQualities.Min(q => q.Revision)) > 0)
                {
                    return ProfileComparisonResult.Equal;
                }
            }

            return ProfileComparisonResult.Upgrade;
        }

        private bool IsPreferredWordUpgradable(int currentScore, int newScore)
        {
            return newScore > currentScore;
        }

        public bool IsUpgradable(QualityProfile qualityProfile, List<QualityModel> currentQualities, int currentScore, QualityModel newQuality, int newScore)
        {

            var qualityUpgrade = IsQualityUpgradable(qualityProfile, currentQualities, newQuality);

            if (qualityUpgrade == ProfileComparisonResult.Upgrade)
            {
                return true;
            }

            if (qualityUpgrade == ProfileComparisonResult.Downgrade)
            {
                _logger.Debug("Existing item has better quality, skipping");
                return false;
            }

            if (!IsPreferredWordUpgradable(currentScore, newScore))
            {
                _logger.Debug("Existing item has a better preferred word score, skipping");
                return false;
            }

            return true;
        }

        public bool QualityCutoffNotMet(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null)
        {
            var cutoffCompare = new QualityModelComparer(profile).Compare(currentQuality.Quality.Id, profile.Cutoff);

            if (cutoffCompare < 0)
            {
                return true;
            }

            if (newQuality != null && IsRevisionUpgrade(currentQuality, newQuality))
            {
                return true;
            }

            return false;
        }

        public bool CutoffNotMet(QualityProfile profile, List<QualityModel> currentQualities, int currentScore, QualityModel newQuality = null, int newScore = 0)
        {
            foreach (var quality in currentQualities)
            {
                if (QualityCutoffNotMet(profile, quality, newQuality))
                {
                    return true;
                }
            }

            if (IsPreferredWordUpgradable(currentScore, newScore))
            {
                return true;
            }

            _logger.Debug("Existing item meets cut-off. skipping.");

            return false;
        }

        public bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality)
        {
            var compare = newQuality.Revision.CompareTo(currentQuality.Revision);

            // Comparing the quality directly because we don't want to upgrade to a proper for a webrip from a webdl or vice versa
            if (currentQuality.Quality == newQuality.Quality && compare > 0)
            {
                _logger.Debug("New quality is a better revision for existing quality");
                return true;
            }

            return false;
        }

        public bool IsUpgradeAllowed(QualityProfile qualityProfile, List<QualityModel> currentQualities, QualityModel newQuality)
        {
            var isQualityUpgrade = IsQualityUpgradable(qualityProfile, currentQualities, newQuality);

            return CheckUpgradeAllowed(qualityProfile, isQualityUpgrade);
        }

        private bool CheckUpgradeAllowed (QualityProfile qualityProfile, ProfileComparisonResult isQualityUpgrade)
        {
            if (isQualityUpgrade == ProfileComparisonResult.Upgrade && !qualityProfile.UpgradeAllowed)
            {
                _logger.Debug("Quality profile does not allow upgrades, skipping");
                return false;
            }

            return true;
        }

        private enum ProfileComparisonResult
        {
            Downgrade = -1,
            Equal = 0,
            Upgrade = 1
        }
    }
}
