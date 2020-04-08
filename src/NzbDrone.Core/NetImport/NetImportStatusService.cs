using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.NetImport
{
    public interface INetImportStatusService : IProviderStatusServiceBase<NetImportStatus>
    {
        Movie GetLastSyncListInfo(int importListId);

        void UpdateListSyncStatus(int importListId, Movie listItemInfo);
    }

    public class NetImportStatusService : ProviderStatusServiceBase<INetImport, NetImportStatus>, INetImportStatusService
    {
        public NetImportStatusService(INetImportStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
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
