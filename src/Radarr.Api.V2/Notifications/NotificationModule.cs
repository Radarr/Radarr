using NzbDrone.Core.Notifications;

namespace Radarr.Api.V2.Notifications
{
    public class NotificationModule : ProviderModuleBase<NotificationResource, INotification, NotificationDefinition>
    {
        public static readonly NotificationResourceMapper ResourceMapper = new NotificationResourceMapper();

        public NotificationModule(NotificationFactory notificationFactory)
            : base(notificationFactory, "notification", ResourceMapper)
        {
        }

        protected override void Validate(NotificationDefinition definition, bool includeWarnings)
        {
            if (!definition.OnGrab && !definition.OnDownload) return;
            base.Validate(definition, includeWarnings);
        }
    }
}
