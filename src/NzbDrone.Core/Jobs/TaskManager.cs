using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Backup;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.Housekeeping;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Update.Commands;

namespace NzbDrone.Core.Jobs
{
    public interface ITaskManager
    {
        IList<ScheduledTask> GetPending();
        List<ScheduledTask> GetAll();
        DateTime GetNextExecution(Type type);
    }

    public class TaskManager : ITaskManager, IHandle<ApplicationStartedEvent>, IHandle<CommandExecutedEvent>, IHandleAsync<ConfigSavedEvent>
    {
        private readonly IScheduledTaskRepository _scheduledTaskRepository;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public TaskManager(IScheduledTaskRepository scheduledTaskRepository, IConfigService configService, Logger logger)
        {
            _scheduledTaskRepository = scheduledTaskRepository;
            _configService = configService;
            _logger = logger;
        }

        public IList<ScheduledTask> GetPending()
        {
            return _scheduledTaskRepository.All()
                                           .Where(c => c.Interval > 0 && c.LastExecution.AddMinutes(c.Interval) < DateTime.UtcNow)
                                           .ToList();
        }

        public List<ScheduledTask> GetAll()
        {
            return _scheduledTaskRepository.All().ToList();
        }

        public DateTime GetNextExecution(Type type)
        {
            var scheduledTask = _scheduledTaskRepository.All().Single(v => v.TypeName == type.FullName);
            return scheduledTask.LastExecution.AddMinutes(scheduledTask.Interval);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            var defaultTasks = new[]
                {
                    new ScheduledTask { Interval = 5, TypeName = typeof(MessagingCleanupCommand).FullName },
                    new ScheduledTask { Interval = 6 * 60, TypeName = typeof(ApplicationCheckUpdateCommand).FullName },
                    new ScheduledTask { Interval = 6 * 60, TypeName = typeof(CheckHealthCommand).FullName },
                    new ScheduledTask { Interval = 24 * 60, TypeName = typeof(RefreshMovieCommand).FullName },
                    new ScheduledTask { Interval = 24 * 60, TypeName = typeof(HousekeepingCommand).FullName },
                    new ScheduledTask { Interval = 24 * 60, TypeName = typeof(CleanUpRecycleBinCommand).FullName },

                    new ScheduledTask
                    {
                        Interval = GetBackupInterval(),
                        TypeName = typeof(BackupCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = GetRssSyncInterval(),
                        TypeName = typeof(RssSyncCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = GetImportListSyncInterval(),
                        TypeName = typeof(ImportListSyncCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = Math.Max(_configService.CheckForFinishedDownloadInterval, 1),
                        TypeName = typeof(RefreshMonitoredDownloadsCommand).FullName
                    }
                };

            var currentTasks = _scheduledTaskRepository.All().ToList();

            _logger.Trace("Initializing jobs. Available: {0} Existing: {1}", defaultTasks.Length, currentTasks.Count);

            foreach (var job in currentTasks)
            {
                if (!defaultTasks.Any(c => c.TypeName == job.TypeName))
                {
                    _logger.Trace("Removing job from database '{0}'", job.TypeName);
                    _scheduledTaskRepository.Delete(job.Id);
                }
            }

            foreach (var defaultTask in defaultTasks)
            {
                var currentDefinition = currentTasks.SingleOrDefault(c => c.TypeName == defaultTask.TypeName) ?? defaultTask;

                currentDefinition.Interval = defaultTask.Interval;

                if (currentDefinition.Id == 0)
                {
                    currentDefinition.LastExecution = DateTime.UtcNow;
                }

                _scheduledTaskRepository.Upsert(currentDefinition);
            }
        }

        private int GetBackupInterval()
        {
            var intervalDays = _configService.BackupInterval;

            if (intervalDays < 1)
            {
                intervalDays = 1;
            }

            if (intervalDays > 7)
            {
                intervalDays = 7;
            }

            return intervalDays * 60 * 24;
        }

        private int GetRssSyncInterval()
        {
            var interval = _configService.RssSyncInterval;

            if (interval > 0 && interval < 10)
            {
                return 10;
            }

            if (interval < 0)
            {
                return 0;
            }

            return interval;
        }

        private int GetImportListSyncInterval()
        {
            //Enforce 6 hour min on list sync
            var interval = Math.Max(_configService.ImportListSyncInterval, 6);

            return interval * 60;
        }

        public void Handle(CommandExecutedEvent message)
        {
            var scheduledTask = _scheduledTaskRepository.All().SingleOrDefault(c => c.TypeName == message.Command.Body.GetType().FullName);

            if (scheduledTask != null && message.Command.Body.UpdateScheduledTask)
            {
                _logger.Trace("Updating last run time for: {0}", scheduledTask.TypeName);
                _scheduledTaskRepository.SetLastExecutionTime(scheduledTask.Id, DateTime.UtcNow, message.Command.StartedAt.Value);
            }
        }

        public void HandleAsync(ConfigSavedEvent message)
        {
            var rss = _scheduledTaskRepository.GetDefinition(typeof(RssSyncCommand));
            rss.Interval = GetRssSyncInterval();

            var importList = _scheduledTaskRepository.GetDefinition(typeof(ImportListSyncCommand));
            importList.Interval = GetImportListSyncInterval();

            var backup = _scheduledTaskRepository.GetDefinition(typeof(BackupCommand));
            backup.Interval = GetBackupInterval();

            var refreshMonitoredDownloads = _scheduledTaskRepository.GetDefinition(typeof(RefreshMonitoredDownloadsCommand));
            refreshMonitoredDownloads.Interval = _configService.CheckForFinishedDownloadInterval;

            _scheduledTaskRepository.UpdateMany(new List<ScheduledTask> { rss, importList, refreshMonitoredDownloads, backup });
        }
    }
}
