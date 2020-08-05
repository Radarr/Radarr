using System.Linq;
using NzbDrone.Core.Indexers.Newznab;

namespace NzbDrone.Core.Indexers.Torznab
{
    public class TorznabRequestGenerator : NewznabRequestGenerator
    {
        public TorznabRequestGenerator(INewznabCapabilitiesProvider capabilitiesProvider)
        : base(capabilitiesProvider)
        {
        }

        protected override bool SupportsBookSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedBookSearchParameters != null &&
                       capabilities.SupportedBookSearchParameters.Contains("q") &&
                       capabilities.SupportedBookSearchParameters.Contains("author") &&
                       capabilities.SupportedBookSearchParameters.Contains("title");
            }
        }
    }
}
