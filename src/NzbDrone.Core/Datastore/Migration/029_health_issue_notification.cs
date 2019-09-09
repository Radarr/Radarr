using FluentMigrator;

using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(29)]
    public class health_issue_notification : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnHealthIssue").AsBoolean().WithDefaultValue(0);
            Alter.Table("Notifications").AddColumn("IncludeHealthWarnings").AsBoolean().WithDefaultValue(0);
            Alter.Table("Notifications").AddColumn("OnDownloadFailure").AsBoolean().WithDefaultValue(0);
            Alter.Table("Notifications").AddColumn("OnImportFailure").AsBoolean().WithDefaultValue(0);
            Alter.Table("Notifications").AddColumn("OnTrackRetag").AsBoolean().WithDefaultValue(0);

            Delete.Column("OnDownload").FromTable("Notifications");
            
            Rename.Column("OnAlbumDownload").OnTable("Notifications").To("OnReleaseImport");
        }
    }
}
