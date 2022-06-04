using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Update.History.Events;

namespace NzbDrone.Core.Update.History
{
    public interface IUpdateHistoryService
    {
        Version PreviouslyInstalled();
        List<UpdateHistory> InstalledSince(DateTime dateTime);
    }

    public class UpdateHistoryService : IUpdateHistoryService, IHandle<ApplicationStartedEvent>, IHandleAsync<ApplicationStartedEvent>
    {
        private readonly IUpdateHistoryRepository _repository;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IUpdatePackageProvider _updatePackageProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;
        private Version _prevVersion;

        public UpdateHistoryService(IUpdateHistoryRepository repository,
                                    IConfigFileProvider configFileProvider,
                                    IUpdatePackageProvider updatePackageProvider,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _repository = repository;
            _configFileProvider = configFileProvider;
            _updatePackageProvider = updatePackageProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public Version PreviouslyInstalled()
        {
            try
            {
                var history = _repository.PreviouslyInstalled();

                return history?.Version;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to determine previously installed version");
                return null;
            }
        }

        public List<UpdateHistory> InstalledSince(DateTime dateTime)
        {
            try
            {
                return _repository.InstalledSince(dateTime);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to get list of previously installed versions");
                return new List<UpdateHistory>();
            }
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (BuildInfo.Version.Major == 10)
            {
                // Don't save dev versions, they change constantly
                return;
            }

            UpdateHistory history;
            try
            {
                history = _repository.LastInstalled();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Cleaning corrupted update history");
                _repository.Purge();
                history = null;
            }

            if (history == null || history.Version != BuildInfo.Version)
                {
                    _prevVersion = history?.Version;

                    _repository.Insert(new UpdateHistory
                    {
                        Date = DateTime.UtcNow,
                        Version = BuildInfo.Version,
                        EventType = UpdateHistoryEventType.Installed
                    });
                }
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            if (_prevVersion != null)
            {
                var branch = _configFileProvider.Branch;
                var version = BuildInfo.Version;
                var packageChanges = _updatePackageProvider.GetRecentUpdates(branch, version, _prevVersion)
                                                           .Select(u => u.Changes);

                var changes = new UpdateChanges();

                foreach (var change in packageChanges)
                {
                    changes.New = change.New.Union(change.New).ToList();
                    changes.Fixed = change.Fixed.Union(change.Fixed).ToList();
                }

                _eventAggregator.PublishEvent(new UpdateInstalledEvent(_prevVersion, BuildInfo.Version, changes));
            }
        }
    }
}
