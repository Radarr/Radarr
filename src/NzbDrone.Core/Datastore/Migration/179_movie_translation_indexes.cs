using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(179)]
    public class movie_translation_indexes : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index("IX_MovieTranslations_Language").OnTable("MovieTranslations").OnColumn("Language");
            Create.Index("IX_MovieTranslations_MovieId").OnTable("MovieTranslations").OnColumn("MovieId");
        }
    }
}
