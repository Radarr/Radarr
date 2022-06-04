using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public interface INotificationFactory : IProviderFactory<INotification, NotificationDefinition>
    {
        List<INotification> OnGrabEnabled();
        List<INotification> OnDownloadEnabled();
        List<INotification> OnUpgradeEnabled();
        List<INotification> OnRenameEnabled();
        List<INotification> OnMovieAddedEnabled();
        List<INotification> OnMovieDeleteEnabled();
        List<INotification> OnMovieFileDeleteEnabled();
        List<INotification> OnHealthIssueEnabled();
        List<INotification> OnApplicationUpdateEnabled();
    }

    public class NotificationFactory : ProviderFactory<INotification, NotificationDefinition>, INotificationFactory
    {
        public NotificationFactory(INotificationRepository providerRepository, IEnumerable<INotification> providers, IServiceProvider container, IEventAggregator eventAggregator, Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
        }

        public List<INotification> OnGrabEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnGrab).ToList();
        }

        public List<INotification> OnDownloadEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnDownload).ToList();
        }

        public List<INotification> OnUpgradeEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnUpgrade).ToList();
        }

        public List<INotification> OnRenameEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnRename).ToList();
        }

        public List<INotification> OnMovieAddedEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnMovieAdded).ToList();
        }

        public List<INotification> OnMovieDeleteEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnMovieDelete).ToList();
        }

        public List<INotification> OnMovieFileDeleteEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnMovieFileDelete).ToList();
        }

        public List<INotification> OnMovieFileDeleteForUpgradeEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnMovieFileDeleteForUpgrade).ToList();
        }

        public List<INotification> OnHealthIssueEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnHealthIssue).ToList();
        }

        public List<INotification> OnApplicationUpdateEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnApplicationUpdate).ToList();
        }

        public override void SetProviderCharacteristics(INotification provider, NotificationDefinition definition)
        {
            base.SetProviderCharacteristics(provider, definition);

            definition.SupportsOnGrab = provider.SupportsOnGrab;
            definition.SupportsOnDownload = provider.SupportsOnDownload;
            definition.SupportsOnUpgrade = provider.SupportsOnUpgrade;
            definition.SupportsOnRename = provider.SupportsOnRename;
            definition.SupportsOnMovieAdded = provider.SupportsOnMovieAdded;
            definition.SupportsOnMovieDelete = provider.SupportsOnMovieDelete;
            definition.SupportsOnMovieFileDelete = provider.SupportsOnMovieFileDelete;
            definition.SupportsOnMovieFileDeleteForUpgrade = provider.SupportsOnMovieFileDeleteForUpgrade;
            definition.SupportsOnHealthIssue = provider.SupportsOnHealthIssue;
            definition.SupportsOnApplicationUpdate = provider.SupportsOnApplicationUpdate;
        }
    }
}
