using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(123)]
    public class update_qualities_and_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {

            //public static Quality Unknown => new Quality(0, "Unknown");
            //public static Quality WORKPRINT => new Quality(24, "WORKPRINT"); // new
            //public static Quality CAM => new Quality(25, "CAM"); // new
            //public static Quality TELESYNC => new Quality(26, "TELESYNC"); // new
            //public static Quality TELECINE => new Quality(27, "TELECINE"); // new
            //public static Quality DVDSCR => new Quality(28, "DVDSCR"); // new
            //public static Quality REGIONAL => new Quality(29, "REGIONAL"); // new
            //public static Quality SDTV => new Quality(1, "SDTV");
            //public static Quality DVD => new Quality(2, "DVD");
            //public static Quality DVDR => new Quality(23, "DVD-R"); // new
            //public static Quality HDTV720p => new Quality(4, "HDTV-720p");
            //public static Quality HDTV1080p => new Quality(9, "HDTV-1080p");
            //public static Quality HDTV2160p => new Quality(16, "HDTV-2160p");
            //public static Quality WEBDL480p => new Quality(8, "WEBDL-480p");
            //public static Quality WEBDL720p => new Quality(5, "WEBDL-720p");
            //public static Quality WEBDL1080p => new Quality(3, "WEBDL-1080p");
            //public static Quality WEBDL2160p => new Quality(18, "WEBDL-2160p");
            //public static Quality Bluray480p => new Quality(20, "Bluray-480p"); // new
            //public static Quality Bluray576p => new Quality(21, "Bluray-576p"); // new
            //public static Quality Bluray720p => new Quality(6, "Bluray-720p");
            //public static Quality Bluray1080p => new Quality(7, "Bluray-1080p");
            //public static Quality Bluray2160p => new Quality(19, "Bluray-2160p");
            //public static Quality BRDISK => new Quality(22, "BR-DISK"); // new
            //public static Quality RAWHD => new Quality(10, "Raw-HD");

            var updater = new ProfileUpdater70(conn, tran);

            // New qualities
            //updater.SplitQualityAppend(0, 24); // WORKPRINT AFTER UNKNOWN
            //updater.SplitQualityAppend(24, 25); // CAM AFTER WORKPRINT
            //updater.SplitQualityAppend(25, 26); // TELESYNC AFTER CAM
            //updater.SplitQualityAppend(26, 27); // TELECINE AFTER TELESYNC
            //updater.SplitQualityAppend(27, 28); // DVDSCR AFTER TELECINE
            //updater.SplitQualityAppend(28, 29); // REGIONAL AFTER DVDSCR

            updater.SplitQualityAppend(0, 27); // TELECINE AFTER Unknown
            updater.SplitQualityAppend(0, 26); // TELESYNC AFTER Unknown
            updater.SplitQualityAppend(0, 25); // CAM AFTER Unknown
            updater.SplitQualityAppend(0, 24); // WORKPRINT AFTER Unknown
            
            updater.SplitQualityPrepend(2, 23); // DVDR     BEFORE     DVD
            updater.SplitQualityPrepend(2, 28); // DVDSCR   BEFORE     DVD
            updater.SplitQualityPrepend(2, 29); // REGIONAL BEFORE     DVD

            updater.SplitQualityAppend(1, 20); // Bluray480p   AFTER     SDTV
            updater.SplitQualityAppend(1, 21); // Bluray576p   AFTER     SDTV

            updater.SplitQualityPrepend(10, 22); // BRDISK       BEFORE      RAWHD

            updater.Commit();

            // WEBRip migrations.
            //updater.SplitQualityAppend(1, 11);   // HDTV480p    after  SDTV
            //updater.SplitQualityPrepend(8, 12);  // WEBRip480p  before WEBDL480p
            //updater.SplitQualityAppend(2, 13);   // Bluray480p  after  DVD
            //updater.SplitQualityPrepend(5, 14);  // WEBRip720p  before WEBDL720p
            //updater.SplitQualityPrepend(3, 15);  // WEBRip1080p before WEBDL1080p
            //updater.SplitQualityPrepend(18, 17); // WEBRip2160p before WEBDL2160p
        }
    }
}
