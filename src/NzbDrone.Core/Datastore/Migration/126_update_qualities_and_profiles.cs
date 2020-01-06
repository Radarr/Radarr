using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(126)]
    public class update_qualities_and_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater70(conn, tran);
            updater.SplitQualityAppend(0, 27); // TELECINE AFTER Unknown
            updater.SplitQualityAppend(0, 26); // TELESYNC AFTER Unknown
            updater.SplitQualityAppend(0, 25); // CAM AFTER Unknown
            updater.SplitQualityAppend(0, 24); // WORKPRINT AFTER Unknown

            updater.SplitQualityPrepend(2, 23); // DVDR     BEFORE     DVD
            updater.SplitQualityPrepend(2, 28); // DVDSCR   BEFORE     DVD
            updater.SplitQualityPrepend(2, 29); // REGIONAL BEFORE     DVD

            updater.SplitQualityAppend(2, 21); // Bluray576p   AFTER     SDTV
            updater.SplitQualityAppend(2, 20); // Bluray480p   AFTER     SDTV

            updater.AppendQuality(22);

            updater.Commit();
        }
    }
}
