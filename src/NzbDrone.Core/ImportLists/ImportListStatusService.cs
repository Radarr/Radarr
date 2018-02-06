using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportListStatusService : IProviderStatusServiceBase<ImportListStatus>
    {
        ImportListItemInfo GetLastSyncListInfo(int importListId);

        void UpdateListSyncStatus(int importListId, ImportListItemInfo listItemInfo);
    }

    public class ImportListStatusService : ProviderStatusServiceBase<IImportList, ImportListStatus>, IImportListStatusService
    {
        public ImportListStatusService(IImportListStatusRepository providerStatusRepository, IEventAggregator eventAggregator, Logger logger)
            : base(providerStatusRepository, eventAggregator, logger)
        {
        }

        public ImportListItemInfo GetLastSyncListInfo(int importListId)
        {
            return GetProviderStatus(importListId).LastSyncListInfo;
        }


        public void UpdateListSyncStatus(int importListId, ImportListItemInfo listItemInfo)
        {
            lock (_syncRoot)
            {
                var status = GetProviderStatus(importListId);

                status.LastSyncListInfo = listItemInfo;

                _providerStatusRepository.Upsert(status);
            }
        }
    }
}
