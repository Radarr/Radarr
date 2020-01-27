using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(134)]
    public class add_remux_qualities_for_the_wankers : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater125(conn, tran);
            updater.SplitQualityAppend(19, 31); // Remux2160p    AFTER     Bluray2160p
            updater.SplitQualityAppend(7, 30);  // Remux1080p    AFTER     Bluray1080p

            updater.Commit();
        }
    }
}
