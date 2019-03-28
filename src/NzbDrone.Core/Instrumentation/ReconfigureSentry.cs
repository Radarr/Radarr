using NLog;
using NzbDrone.Common.Instrumentation.Sentry;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Instrumentation
{
    public class ReconfigureSentry : IHandleAsync<ApplicationStartedEvent>
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IMainDatabase _database;

        public ReconfigureSentry(IConfigFileProvider configFileProvider,
                                  IMainDatabase database)
        {
            _configFileProvider = configFileProvider;
            _database = database;
        }

        public void Reconfigure()
        {
            // Extended sentry config
            var sentry = LogManager.Configuration.FindTargetByName<SentryTarget>("sentryTarget");
            sentry.FilterEvents = _configFileProvider.FilterSentryEvents;
            sentry.UpdateBranch = _configFileProvider.Branch;
            sentry.DatabaseVersion = _database.Version;
            sentry.DatabaseMigration = _database.Migration;

            LogManager.ReconfigExistingLoggers();
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            Reconfigure();
        }
    }
}
