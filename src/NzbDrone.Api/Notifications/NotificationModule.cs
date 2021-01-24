using NzbDrone.Core.Notifications;

namespace NzbDrone.Api.Notifications
{
    public class NotificationModule : ProviderModuleBase<NotificationResource, INotification, NotificationDefinition>
    {
        public NotificationModule(NotificationFactory notificationFactory)
            : base(notificationFactory, "notification")
        {
        }

        protected override void MapToResource(NotificationResource resource, NotificationDefinition definition)
        {
            base.MapToResource(resource, definition);

            resource.OnGrab = definition.OnGrab;
            resource.OnDownload = definition.OnDownload;
            resource.OnUpgrade = definition.OnUpgrade;
            resource.OnRename = definition.OnRename;
            resource.OnMovieDelete = definition.OnMovieDelete;
            resource.OnMovieFileDelete = definition.OnMovieFileDelete;
            resource.OnMovieFileDeleteForUpgrade = definition.OnMovieFileDeleteForUpgrade;
            resource.SupportsOnGrab = definition.SupportsOnGrab;
            resource.SupportsOnDownload = definition.SupportsOnDownload;
            resource.SupportsOnUpgrade = definition.SupportsOnUpgrade;
            resource.SupportsOnRename = definition.SupportsOnRename;
            resource.SupportsOnMovieDelete = definition.SupportsOnMovieDelete;
            resource.SupportsOnMovieFileDelete = definition.SupportsOnMovieFileDelete;
            resource.SupportsOnMovieFileDeleteForUpgrade = definition.SupportsOnMovieFileDeleteForUpgrade;
            resource.Tags = definition.Tags;
        }

        protected override void MapToModel(NotificationDefinition definition, NotificationResource resource)
        {
            base.MapToModel(definition, resource);

            definition.OnGrab = resource.OnGrab;
            definition.OnDownload = resource.OnDownload;
            definition.OnUpgrade = resource.OnUpgrade;
            definition.OnRename = resource.OnRename;
            definition.OnMovieDelete = resource.OnMovieDelete;
            definition.OnMovieFileDelete = resource.OnMovieFileDelete;
            definition.OnMovieFileDeleteForUpgrade = resource.OnMovieFileDeleteForUpgrade;
            definition.SupportsOnGrab = resource.SupportsOnGrab;
            definition.SupportsOnDownload = resource.SupportsOnDownload;
            definition.SupportsOnUpgrade = resource.SupportsOnUpgrade;
            definition.SupportsOnRename = resource.SupportsOnRename;
            definition.SupportsOnMovieDelete = resource.SupportsOnMovieDelete;
            definition.SupportsOnMovieFileDelete = resource.SupportsOnMovieFileDelete;
            definition.SupportsOnMovieFileDeleteForUpgrade = resource.SupportsOnMovieFileDeleteForUpgrade;
            definition.Tags = resource.Tags;
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
