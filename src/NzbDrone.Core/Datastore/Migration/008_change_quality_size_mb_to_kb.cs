using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(8)]
    public class change_quality_size_mb_to_kb : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE QualityDefinitions SET MaxSize = CASE " +
                        "WHEN (CAST(MaxSize AS FLOAT) / 60) * 8 * 1024 < 1500 THEN " +
                        "ROUND((CAST(MaxSize AS FLOAT) / 60) * 8 * 1024, 0) " +
                        "ELSE NULL " +
                        "END");
        }
    }
}
