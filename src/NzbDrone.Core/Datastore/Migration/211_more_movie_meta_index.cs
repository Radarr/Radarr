using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(211)]
    public class more_movie_meta_index : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index("IX_AlternativeTitles_MovieMetadataId").OnTable("AlternativeTitles").OnColumn("MovieMetadataId");
            Create.Index("IX_Credits_MovieMetadataId").OnTable("Credits").OnColumn("MovieMetadataId");
        }
    }
}
