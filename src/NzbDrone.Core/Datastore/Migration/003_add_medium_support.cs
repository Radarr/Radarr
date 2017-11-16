using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(3)]
    public class add_medium_support : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Albums").AddColumn("Media").AsString().WithDefaultValue("");
            Alter.Table("Tracks").AddColumn("MediumNumber").AsInt32().WithDefaultValue(0);
            Alter.Table("Tracks").AddColumn("AbsoluteTrackNumber").AsInt32().WithDefaultValue(0);

            Execute.Sql("UPDATE Tracks SET AbsoluteTrackNumber = TrackNumber");
            
            Delete.Column("TrackNumber").FromTable("Tracks");
            Alter.Table("Tracks").AddColumn("TrackNumber").AsString().Nullable();
            
        }
    }
}
