using NzbDrone.Core.Notifications;

namespace Lidarr.Api.V1.Notifications
{
    public class NotificationResource : ProviderResource
    {
        public string Link { get; set; }
        public bool OnGrab { get; set; }
        public bool OnReleaseImport { get; set; }
        public bool OnUpgrade { get; set; }
        public bool OnRename { get; set; }
        public bool OnHealthIssue { get; set; }
        public bool OnDownloadFailure { get; set; }
        public bool OnImportFailure { get; set; }
        public bool OnTrackRetag { get; set; }
        public bool SupportsOnGrab { get; set; }
        public bool SupportsOnReleaseImport { get; set; }
        public bool SupportsOnUpgrade { get; set; }
        public bool SupportsOnRename { get; set; }
        public bool SupportsOnHealthIssue { get; set; }
        public bool IncludeHealthWarnings { get; set; }
        public bool SupportsOnDownloadFailure { get; set; }
        public bool SupportsOnImportFailure { get; set; }
        public bool SupportsOnTrackRetag { get; set; }
        public string TestCommand { get; set; }
    }

    public class NotificationResourceMapper : ProviderResourceMapper<NotificationResource, NotificationDefinition>
    {
        public override NotificationResource ToResource(NotificationDefinition definition)
        {
            if (definition == null) return default(NotificationResource);

            var resource = base.ToResource(definition);

            resource.OnGrab = definition.OnGrab;
            resource.OnReleaseImport = definition.OnReleaseImport;
            resource.OnUpgrade = definition.OnUpgrade;
            resource.OnRename = definition.OnRename;
            resource.OnHealthIssue = definition.OnHealthIssue;
            resource.OnDownloadFailure = definition.OnDownloadFailure;
            resource.OnImportFailure = definition.OnImportFailure;
            resource.OnTrackRetag = definition.OnTrackRetag;
            resource.SupportsOnGrab = definition.SupportsOnGrab;
            resource.SupportsOnReleaseImport = definition.SupportsOnReleaseImport;
            resource.SupportsOnUpgrade = definition.SupportsOnUpgrade;
            resource.SupportsOnRename = definition.SupportsOnRename;
            resource.SupportsOnHealthIssue = definition.SupportsOnHealthIssue;
            resource.IncludeHealthWarnings = definition.IncludeHealthWarnings;
            resource.SupportsOnDownloadFailure = definition.SupportsOnDownloadFailure;
            resource.SupportsOnImportFailure = definition.SupportsOnImportFailure;
            resource.SupportsOnTrackRetag = definition.SupportsOnTrackRetag;

            return resource;
        }

        public override NotificationDefinition ToModel(NotificationResource resource)
        {
            if (resource == null) return default(NotificationDefinition);

            var definition = base.ToModel(resource);

            definition.OnGrab = resource.OnGrab;
            definition.OnReleaseImport = resource.OnReleaseImport;
            definition.OnUpgrade = resource.OnUpgrade;
            definition.OnRename = resource.OnRename;
            definition.OnHealthIssue = resource.OnHealthIssue;
            definition.OnDownloadFailure = resource.OnDownloadFailure;
            definition.OnImportFailure = resource.OnImportFailure;
            definition.OnTrackRetag = resource.OnTrackRetag;
            definition.SupportsOnGrab = resource.SupportsOnGrab;
            definition.SupportsOnReleaseImport = resource.SupportsOnReleaseImport;
            definition.SupportsOnUpgrade = resource.SupportsOnUpgrade;
            definition.SupportsOnRename = resource.SupportsOnRename;
            definition.SupportsOnHealthIssue = resource.SupportsOnHealthIssue;
            definition.IncludeHealthWarnings = resource.IncludeHealthWarnings;
            definition.SupportsOnDownloadFailure = resource.SupportsOnDownloadFailure;
            definition.SupportsOnImportFailure = resource.SupportsOnImportFailure;
            definition.SupportsOnTrackRetag = resource.SupportsOnTrackRetag;

            return definition;
        }
    }
}
