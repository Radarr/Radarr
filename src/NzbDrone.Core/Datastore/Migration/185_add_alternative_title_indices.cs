using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(185)]
    public class add_alternative_title_indices : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index().OnTable("AlternativeTitles").OnColumn("CleanTitle");
            Create.Index().OnTable("MovieTranslations").OnColumn("CleanTitle");
        }
    }
}
