using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(14)]
    public class fix_language_metadata_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE artists SET metadataProfileId = " +
                        "CASE WHEN ((SELECT COUNT(*) FROM metadataprofiles) > 0) " +
                        "THEN (SELECT id FROM metadataprofiles ORDER BY id ASC LIMIT 1) " +
                        "ELSE 0 END " +
                        "WHERE artists.metadataProfileId == 0");

            Execute.Sql("UPDATE artists SET languageProfileId = " +
                        "CASE WHEN ((SELECT COUNT(*) FROM languageprofiles) > 0) " +
                        "THEN (SELECT id FROM languageprofiles ORDER BY id ASC LIMIT 1) " +
                        "ELSE 0 END " +
                        "WHERE artists.languageProfileId == 0");
        }
    }
}
