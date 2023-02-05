using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class HardcodeSubsSpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public HardcodeSubsSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var hardcodeSubs = subject.ParsedMovieInfo.HardcodedSubs;

            if (_configService.AllowHardcodedSubs || hardcodeSubs.IsNullOrWhiteSpace())
            {
                return Decision.Accept();
            }

            var whitelisted = _configService.WhitelistedHardcodedSubs.Split(',');

            if (whitelisted != null && whitelisted.Any(t => (hardcodeSubs.ToLower().Contains(t.ToLower()) && t.IsNotNullOrWhiteSpace())))
            {
                _logger.Debug("Release hardcode subs ({0}) are in allowed values ({1})", hardcodeSubs, whitelisted);
                return Decision.Accept();
            }
            else
            {
                _logger.Debug("Hardcode subs found: {0}", hardcodeSubs);
                return Decision.Reject("Hardcode subs found: {0}", hardcodeSubs);
            }
        }
    }
}
