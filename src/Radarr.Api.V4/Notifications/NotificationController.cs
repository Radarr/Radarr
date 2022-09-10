using NzbDrone.Core.Notifications;
using Radarr.Http;

namespace Radarr.Api.V4.Notifications
{
    [V4ApiController]
    public class NotificationController : ProviderControllerBase<NotificationResource, NotificationBulkResource, INotification, NotificationDefinition>
    {
        public static readonly NotificationResourceMapper ResourceMapper = new NotificationResourceMapper();
        public static readonly NotificationBulkResourceMapper BulkResourceMapper = new NotificationBulkResourceMapper();

        public NotificationController(NotificationFactory notificationFactory)
            : base(notificationFactory, "notification", ResourceMapper, BulkResourceMapper)
        {
        }
    }
}
