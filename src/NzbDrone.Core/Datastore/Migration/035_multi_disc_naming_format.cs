using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Data;
using System.IO;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(35)]
    public class multi_disc_naming_format : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("MultiDiscTrackFormat").AsString().Nullable();
            Execute.Sql("UPDATE NamingConfig SET MultiDiscTrackFormat = '{Medium Format} {medium:00}/{Artist Name} - {Album Title} - {track:00} - {Track Title}'");
        }
    }
}
