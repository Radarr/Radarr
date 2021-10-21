using NzbDrone.Core.Notifications;
using Radarr.Http;

namespace Radarr.Api.V3.Notifications
{
    [V3ApiController]
    public class NotificationController : ProviderControllerBase<NotificationResource, INotification, NotificationDefinition>
    {
        public static readonly NotificationResourceMapper ResourceMapper = new NotificationResourceMapper();

        public NotificationController(NotificationFactory notificationFactory)
            : base(notificationFactory, "notification", ResourceMapper)
        {
        }

        protected override void Validate(NotificationDefinition definition, bool includeWarnings)
        {
            if (!definition.OnGrab && !definition.OnDownload)
            {
                return;
            }

            base.Validate(definition, includeWarnings);
        }
    }
}
