using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Datastore;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Blacklist
{
    public class BlacklistModule : ReadarrRestModule<BlacklistResource>
    {
        private readonly IBlacklistService _blacklistService;

        public BlacklistModule(IBlacklistService blacklistService)
        {
            _blacklistService = blacklistService;
            GetResourcePaged = GetBlacklist;
            DeleteResource = DeleteBlacklist;

            Delete("/bulk", x => Remove());
        }

        private PagingResource<BlacklistResource> GetBlacklist(PagingResource<BlacklistResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<BlacklistResource, NzbDrone.Core.Blacklisting.Blacklist>("date", SortDirection.Descending);

            return ApplyToPage(_blacklistService.Paged, pagingSpec, BlacklistResourceMapper.MapToResource);
        }

        private void DeleteBlacklist(int id)
        {
            _blacklistService.Delete(id);
        }

        private object Remove()
        {
            var resource = Request.Body.FromJson<BlacklistBulkResource>();

            _blacklistService.Delete(resource.Ids);

            return new object();
        }
    }
}
