using FluentMigrator;
//using FluentMigrator.Expressions;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(135)]
    public class update_metadata_files : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {

            Alter.Table("MetadataFiles").AddColumn("MovieId").AsInt32();
            Alter.Table("MetadataFiles").AddColumn("MovieFileId").AsInt32();

            Alter.Table("ExtraFiles").AddColumn("MovieId").AsInt32();
            Alter.Table("ExtraFiles").AddColumn("MovieFileId").AsInt32();

            Alter.Table("SubtitleFiles").AddColumn("MovieId").AsInt32();
            Alter.Table("SubtitleFiles").AddColumn("MovieFileId").AsInt32();

            // We can migrate these back in over time
            Delete.FromTable("Metadata").Row(new { Implementation = "WdtvMetadata" });
            Delete.FromTable("Metadata").Row(new { Implementation = "RoksboxMetadata" });
            Delete.FromTable("Metadata").Row(new { Implementation = "MediaBrowserMetadata" });
        }
    }
}
