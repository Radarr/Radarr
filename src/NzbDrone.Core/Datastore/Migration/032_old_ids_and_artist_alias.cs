using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(32)]
    public class old_ids_and_artist_alias : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ArtistMetadata").AddColumn("Aliases").AsString().WithDefaultValue("[]");
            
            Alter.Table("ArtistMetadata").AddColumn("OldForeignArtistIds").AsString().WithDefaultValue("[]");
            Alter.Table("Albums").AddColumn("OldForeignAlbumIds").AsString().WithDefaultValue("[]");
            Alter.Table("AlbumReleases").AddColumn("OldForeignReleaseIds").AsString().WithDefaultValue("[]");
            Alter.Table("Tracks").AddColumn("OldForeignRecordingIds").AsString().WithDefaultValue("[]");
            Alter.Table("Tracks").AddColumn("OldForeignTrackIds").AsString().WithDefaultValue("[]");
        }
    }
}
