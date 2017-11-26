using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(5)]
    public class metadata_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("MetadataProfiles")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("PrimaryAlbumTypes").AsString()
                .WithColumn("SecondaryAlbumTypes").AsString();

            Alter.Table("Artists").AddColumn("MetadataProfileId").AsInt32().WithDefaultValue(1);

            Delete.Column("PrimaryAlbumTypes").FromTable("Artists");
            Delete.Column("SecondaryAlbumTypes").FromTable("Artists");

            Alter.Table("Albums").AddColumn("SecondaryTypes").AsString().Nullable();

        }
    }
}
