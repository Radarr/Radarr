using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Datastore;
using Radarr.Http;

namespace Radarr.Api.V2.Blacklist
{
    public class BlacklistModule : RadarrRestModule<BlacklistResource>
    {
        private readonly IBlacklistService _blacklistService;

        public BlacklistModule(IBlacklistService blacklistService)
        {
            _blacklistService = blacklistService;
            GetResourcePaged = GetBlacklist;
            DeleteResource = DeleteBlacklist;
        }

        private PagingResource<BlacklistResource> GetBlacklist(PagingResource<BlacklistResource> pagingResource)
        {
            var pagingSpec = new PagingSpec<NzbDrone.Core.Blacklisting.Blacklist>
                                 {
                                     Page = pagingResource.Page,
                                     PageSize = pagingResource.PageSize,
                                     SortKey = pagingResource.SortKey,
                                     SortDirection = pagingResource.SortDirection
                                 };

            return ApplyToPage(_blacklistService.Paged, pagingSpec, BlacklistResourceMapper.MapToResource);
        }

        private void DeleteBlacklist(int id)
        {
            _blacklistService.Delete(id);
        }
    }
}
