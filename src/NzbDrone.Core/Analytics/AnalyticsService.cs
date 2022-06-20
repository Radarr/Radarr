using System;
using System.Linq;
using Microsoft.Extensions.Options;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.History;

namespace NzbDrone.Core.Analytics
{
    public interface IAnalyticsService
    {
        bool IsEnabled { get; }
        bool InstallIsActive { get; }
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileOptions;
        private readonly IHistoryService _historyService;

        public AnalyticsService(IHistoryService historyService, IOptionsMonitor<ConfigFileOptions> configFileOptions)
        {
            _configFileOptions = configFileOptions;
            _historyService = historyService;
        }

        public bool IsEnabled => (_configFileOptions.CurrentValue.AnalyticsEnabled && RuntimeInfo.IsProduction) || RuntimeInfo.IsDevelopment;

        public bool InstallIsActive
        {
            get
            {
                var lastRecord = _historyService.Paged(new PagingSpec<MovieHistory>() { Page = 0, PageSize = 1, SortKey = "date", SortDirection = SortDirection.Descending });
                var monthAgo = DateTime.UtcNow.AddMonths(-1);

                return lastRecord.Records.Any(v => v.Date > monthAgo);
            }
        }
    }
}
