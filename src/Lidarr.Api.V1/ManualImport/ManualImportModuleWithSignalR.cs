using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles.TrackImport.Manual;
using NzbDrone.Core.Qualities;
using Lidarr.Http;
using Lidarr.Http.Extensions;
using NzbDrone.SignalR;
using NLog;

namespace Lidarr.Api.V1.ManualImport
{
    public abstract class ManualImportModuleWithSignalR : LidarrRestModuleWithSignalR<ManualImportResource, ManualImportItem>
    {
        protected readonly IManualImportService _manualImportService;
        protected readonly Logger _logger;

        protected ManualImportModuleWithSignalR(IManualImportService manualImportService,
                                                IBroadcastSignalRMessage signalRBroadcaster,
                                                Logger logger)
        : base(signalRBroadcaster)
        {
            _manualImportService = manualImportService;
            _logger = logger;

            GetResourceById = GetManualImportItem;
        }

        protected ManualImportModuleWithSignalR(IManualImportService manualImportService,
                                                IBroadcastSignalRMessage signalRBroadcaster,
                                                Logger logger,
                                                string resource)
        : base(signalRBroadcaster, resource)
        {
            _manualImportService = manualImportService;
            _logger = logger;

            GetResourceById = GetManualImportItem;
        }

        protected ManualImportResource GetManualImportItem(int id)
        {
            return _manualImportService.Find(id).ToResource();
        }
    }
}
