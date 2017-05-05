using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(112)]
    public class add_music_fields_to_namingconfig : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("ArtistFolderFormat").AsAnsiString().Nullable();
            Alter.Table("NamingConfig").AddColumn("AlbumFolderFormat").AsAnsiString().Nullable();
        }
    }
}
