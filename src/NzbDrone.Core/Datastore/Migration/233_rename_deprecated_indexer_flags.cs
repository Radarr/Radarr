using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(233)]
    public class rename_deprecated_indexer_flags : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"DownloadHistory\" SET \"Release\" = REPLACE(REPLACE(\"Release\", 'hdB_Internal', 'g_Internal'), 'ahD_Internal', 'g_Internal') WHERE \"Release\" IS NOT NULL");
            Execute.Sql("UPDATE \"IndexerStatus\" SET \"LastRssSyncReleaseInfo\" = REPLACE(REPLACE(\"LastRssSyncReleaseInfo\", 'hdB_Internal', 'g_Internal'), 'ahD_Internal', 'g_Internal') WHERE \"LastRssSyncReleaseInfo\" IS NOT NULL");
            Execute.Sql("UPDATE \"PendingReleases\" SET \"Release\" = REPLACE(REPLACE(\"Release\", 'hdB_Internal', 'g_Internal'), 'ahD_Internal', 'g_Internal') WHERE \"Release\" IS NOT NULL");
            Execute.Sql("UPDATE \"History\" SET \"Data\" = REPLACE(REPLACE(\"Data\", 'HDB_Internal', 'G_Internal'), 'AHD_Internal', 'G_Internal') WHERE \"Data\" IS NOT NULL");
        }
    }
}
