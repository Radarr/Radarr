using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportListStatusService : IProviderStatusServiceBase<ImportListStatus>
    {
        Movie GetLastSyncListInfo(int importListId);

        void UpdateListSyncStatus(int importListId, Movie listItemInfo);
    }

    public class ImportListStatusService : ProviderStatusServiceBase<IImportList, ImportListStatus>, IImportListStatusService
    {
        public ImportListStatusService(IImportListStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
        }

        public Movie GetLastSyncListInfo(int importListId)
        {
            return GetProviderStatus(importListId).LastSyncListInfo;
        }

        public void UpdateListSyncStatus(int importListId, Movie listItemInfo)
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
