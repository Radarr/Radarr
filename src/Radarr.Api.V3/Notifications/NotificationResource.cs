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
        public bool OnMovieAdded { get; set; }
        public bool OnMovieDelete { get; set; }
        public bool OnMovieFileDelete { get; set; }
        public bool OnMovieFileDeleteForUpgrade { get; set; }
        public bool OnHealthIssue { get; set; }
        public bool IncludeHealthWarnings { get; set; }
        public bool OnHealthRestored { get; set; }
        public bool OnApplicationUpdate { get; set; }
        public bool OnManualInteractionRequired { get; set; }
        public bool SupportsOnGrab { get; set; }
        public bool SupportsOnDownload { get; set; }
        public bool SupportsOnUpgrade { get; set; }
        public bool SupportsOnRename { get; set; }
        public bool SupportsOnMovieAdded { get; set; }
        public bool SupportsOnMovieDelete { get; set; }
        public bool SupportsOnMovieFileDelete { get; set; }
        public bool SupportsOnMovieFileDeleteForUpgrade { get; set; }
        public bool SupportsOnHealthIssue { get; set; }
        public bool SupportsOnHealthRestored { get; set; }
        public bool SupportsOnApplicationUpdate { get; set; }
        public bool SupportsOnManualInteractionRequired { get; set; }
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
            resource.OnMovieAdded = definition.OnMovieAdded;
            resource.OnMovieDelete = definition.OnMovieDelete;
            resource.OnMovieFileDelete = definition.OnMovieFileDelete;
            resource.OnMovieFileDeleteForUpgrade = definition.OnMovieFileDeleteForUpgrade;
            resource.OnHealthIssue = definition.OnHealthIssue;
            resource.IncludeHealthWarnings = definition.IncludeHealthWarnings;
            resource.OnHealthRestored = definition.OnHealthRestored;
            resource.OnApplicationUpdate = definition.OnApplicationUpdate;
            resource.OnManualInteractionRequired = definition.OnManualInteractionRequired;
            resource.SupportsOnGrab = definition.SupportsOnGrab;
            resource.SupportsOnDownload = definition.SupportsOnDownload;
            resource.SupportsOnUpgrade = definition.SupportsOnUpgrade;
            resource.SupportsOnRename = definition.SupportsOnRename;
            resource.SupportsOnMovieAdded = definition.SupportsOnMovieAdded;
            resource.SupportsOnMovieDelete = definition.SupportsOnMovieDelete;
            resource.SupportsOnMovieFileDelete = definition.SupportsOnMovieFileDelete;
            resource.SupportsOnMovieFileDeleteForUpgrade = definition.SupportsOnMovieFileDeleteForUpgrade;
            resource.SupportsOnHealthIssue = definition.SupportsOnHealthIssue;
            resource.SupportsOnHealthRestored = definition.SupportsOnHealthRestored;
            resource.SupportsOnApplicationUpdate = definition.SupportsOnApplicationUpdate;
            resource.SupportsOnManualInteractionRequired = definition.SupportsOnManualInteractionRequired;

            return resource;
        }

        public override NotificationDefinition ToModel(NotificationResource resource, NotificationDefinition existingDefinition)
        {
            if (resource == null)
            {
                return default(NotificationDefinition);
            }

            var definition = base.ToModel(resource, existingDefinition);

            definition.OnGrab = resource.OnGrab;
            definition.OnDownload = resource.OnDownload;
            definition.OnUpgrade = resource.OnUpgrade;
            definition.OnRename = resource.OnRename;
            definition.OnMovieAdded = resource.OnMovieAdded;
            definition.OnMovieDelete = resource.OnMovieDelete;
            definition.OnMovieFileDelete = resource.OnMovieFileDelete;
            definition.OnMovieFileDeleteForUpgrade = resource.OnMovieFileDeleteForUpgrade;
            definition.OnHealthIssue = resource.OnHealthIssue;
            definition.IncludeHealthWarnings = resource.IncludeHealthWarnings;
            definition.OnHealthRestored = resource.OnHealthRestored;
            definition.OnApplicationUpdate = resource.OnApplicationUpdate;
            definition.OnManualInteractionRequired = resource.OnManualInteractionRequired;
            definition.SupportsOnGrab = resource.SupportsOnGrab;
            definition.SupportsOnDownload = resource.SupportsOnDownload;
            definition.SupportsOnUpgrade = resource.SupportsOnUpgrade;
            definition.SupportsOnRename = resource.SupportsOnRename;
            definition.SupportsOnMovieAdded = resource.SupportsOnMovieAdded;
            definition.SupportsOnMovieDelete = resource.SupportsOnMovieDelete;
            definition.SupportsOnMovieFileDelete = resource.SupportsOnMovieFileDelete;
            definition.SupportsOnMovieFileDeleteForUpgrade = resource.SupportsOnMovieFileDeleteForUpgrade;
            definition.SupportsOnHealthIssue = resource.SupportsOnHealthIssue;
            definition.SupportsOnHealthRestored = resource.SupportsOnHealthRestored;
            definition.SupportsOnApplicationUpdate = resource.SupportsOnApplicationUpdate;
            definition.SupportsOnManualInteractionRequired = resource.SupportsOnManualInteractionRequired;

            return definition;
        }
    }
}
