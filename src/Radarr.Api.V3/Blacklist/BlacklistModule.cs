using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using Radarr.Http;

namespace Radarr.Api.V3.Blacklist
{
    public class BlacklistModule : RadarrRestModule<BlacklistResource>
    {
        private readonly IBlacklistService _blacklistService;
        private readonly ICustomFormatCalculationService _formatCalculator;

        public BlacklistModule(IBlacklistService blacklistService,
                               ICustomFormatCalculationService formatCalculator)
        {
            _blacklistService = blacklistService;
            _formatCalculator = formatCalculator;

            GetResourcePaged = GetBlacklist;
            DeleteResource = DeleteBlacklist;
        }

        private PagingResource<BlacklistResource> GetBlacklist(PagingResource<BlacklistResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<BlacklistResource, NzbDrone.Core.Blacklisting.Blacklist>("date", SortDirection.Descending);

            return ApplyToPage(_blacklistService.Paged, pagingSpec, (blacklist) => BlacklistResourceMapper.MapToResource(blacklist, _formatCalculator));
        }

        private void DeleteBlacklist(int id)
        {
            _blacklistService.Delete(id);
        }
    }
}
