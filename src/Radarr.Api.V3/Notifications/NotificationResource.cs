using NzbDrone.Core.Notifications;

namespace Radarr.Api.V3.Notifications
{
    public class NotificationResource : ProviderResource<NotificationResource>
    {
        public string Link { get; set; }
        public bool OnGrab { get; set; }
        public bool OnDownload { get; set; }
        public bool OnUpgrade { get; set; }
        public bool OnRename { get; set; }
        public bool OnMovieDelete { get; set; }
        public bool OnMovieFileDelete { get; set; }
        public bool OnMovieFileDeleteForUpgrade { get; set; }
        public bool OnHealthIssue { get; set; }
        public bool OnApplciationUpdate { get; set; }
        public bool SupportsOnGrab { get; set; }
        public bool SupportsOnDownload { get; set; }
        public bool SupportsOnUpgrade { get; set; }
        public bool SupportsOnRename { get; set; }
        public bool SupportsOnMovieDelete { get; set; }
        public bool SupportsOnMovieFileDelete { get; set; }
        public bool SupportsOnMovieFileDeleteForUpgrade { get; set; }
        public bool SupportsOnHealthIssue { get; set; }
        public bool SupportsOnApplicationUpdate { get; set; }
        public bool IncludeHealthWarnings { get; set; }
        public string TestCommand { get; set; }
    }

    public class NotificationResourceMapper : ProviderResourceMapper<NotificationResource, NotificationDefinition>
    {
        public override NotificationResource ToResource(NotificationDefinition definition)
        {
            if (definition == null)
            {
                return default(NotificationResource);
            }

            var resource = base.ToResource(definition);

            resource.OnGrab = definition.OnGrab;
            resource.OnDownload = definition.OnDownload;
            resource.OnUpgrade = definition.OnUpgrade;
            resource.OnRename = definition.OnRename;
            resource.OnMovieDelete = definition.OnMovieDelete;
            resource.OnMovieFileDelete = definition.OnMovieFileDelete;
            resource.OnMovieFileDeleteForUpgrade = definition.OnMovieFileDeleteForUpgrade;
            resource.OnHealthIssue = definition.OnHealthIssue;
            resource.OnApplciationUpdate = definition.OnApplicationUpdate;
            resource.SupportsOnGrab = definition.SupportsOnGrab;
            resource.SupportsOnDownload = definition.SupportsOnDownload;
            resource.SupportsOnUpgrade = definition.SupportsOnUpgrade;
            resource.SupportsOnRename = definition.SupportsOnRename;
            resource.SupportsOnMovieDelete = definition.SupportsOnMovieDelete;
            resource.SupportsOnMovieFileDelete = definition.SupportsOnMovieFileDelete;
            resource.SupportsOnMovieFileDeleteForUpgrade = definition.SupportsOnMovieFileDeleteForUpgrade;
            resource.SupportsOnHealthIssue = definition.SupportsOnHealthIssue;
            resource.IncludeHealthWarnings = definition.IncludeHealthWarnings;
            resource.SupportsOnApplicationUpdate = definition.SupportsOnApplicationUpdate;

            return resource;
        }

        public override NotificationDefinition ToModel(NotificationResource resource)
        {
            if (resource == null)
            {
                return default(NotificationDefinition);
            }

            var definition = base.ToModel(resource);

            definition.OnGrab = resource.OnGrab;
            definition.OnDownload = resource.OnDownload;
            definition.OnUpgrade = resource.OnUpgrade;
            definition.OnRename = resource.OnRename;
            definition.OnMovieDelete = resource.OnMovieDelete;
            definition.OnMovieFileDelete = resource.OnMovieFileDelete;
            definition.OnMovieFileDeleteForUpgrade = resource.OnMovieFileDeleteForUpgrade;
            definition.OnHealthIssue = resource.OnHealthIssue;
            definition.OnApplicationUpdate = resource.OnApplciationUpdate;
            definition.SupportsOnGrab = resource.SupportsOnGrab;
            definition.SupportsOnDownload = resource.SupportsOnDownload;
            definition.SupportsOnUpgrade = resource.SupportsOnUpgrade;
            definition.SupportsOnRename = resource.SupportsOnRename;
            definition.SupportsOnMovieDelete = resource.SupportsOnMovieDelete;
            definition.SupportsOnMovieFileDelete = resource.SupportsOnMovieFileDelete;
            definition.SupportsOnMovieFileDeleteForUpgrade = resource.SupportsOnMovieFileDeleteForUpgrade;
            definition.SupportsOnHealthIssue = resource.SupportsOnHealthIssue;
            definition.IncludeHealthWarnings = resource.IncludeHealthWarnings;
            definition.SupportsOnApplicationUpdate = resource.SupportsOnApplicationUpdate;

            return definition;
        }
    }
}
