using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(108)]
    public class update_schedule_intervale : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ScheduledTasks").AlterColumn("Interval").AsDouble();
            Execute.Sql("UPDATE \"ScheduledTasks\" SET \"Interval\" = 0.25 WHERE \"TypeName\" = 'NzbDrone.Core.Download.CheckForFinishedDownloadCommand'");
        }
    }
}
