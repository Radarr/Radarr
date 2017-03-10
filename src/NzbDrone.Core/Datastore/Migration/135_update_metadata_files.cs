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
            if (!Schema.Schema("dbo").Table("MetadataFiles").Column("MovieId").Exists())
            {
                Alter.Table("MetadataFiles").AddColumn("MovieId").AsInt32();
            }
            if (!Schema.Schema("dbo").Table("MetadataFiles").Column("MovieFileId").Exists())
            {
                Alter.Table("MetadataFiles").AddColumn("MovieFileId").AsInt32();
            }

            Delete.FromTable("Metadata").Row(new { Implementation = "WdtvMetadata" });
            Delete.FromTable("Metadata").Row(new { Implementation = "RoksboxMetadata" });
            Delete.FromTable("Metadata").Row(new { Implementation = "MediaBrowserMetadata" });
        }
    }
}
