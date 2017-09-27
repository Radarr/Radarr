using NLog;
using System.Linq;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class LanguageSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public LanguageSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            var wantedLanguage = subject.Artist.LanguageProfile.Value.Languages;
            var _language = subject.ParsedAlbumInfo.Language;

            _logger.Debug("Checking if report meets language requirements. {0}", subject.ParsedAlbumInfo.Language);

            if (!wantedLanguage.Exists(v => v.Allowed && v.Language == _language))
            {
                _logger.Debug("Report Language: {0} rejected because it is not wanted, wanted {1}", subject.ParsedAlbumInfo.Language, wantedLanguage);
                return Decision.Reject("{0} is wanted, but found {1}", wantedLanguage, subject.ParsedAlbumInfo.Language);
            }

            return Decision.Accept();
        }
    }
}
