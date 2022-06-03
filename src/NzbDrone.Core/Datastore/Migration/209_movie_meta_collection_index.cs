using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(209)]
    public class movie_meta_collection_index : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index("IX_MovieMetadata_CollectionTmdbId").OnTable("MovieMetadata").OnColumn("CollectionTmdbId");
            Create.Index("IX_MovieTranslations_MovieMetadataId").OnTable("MovieTranslations").OnColumn("MovieMetadataId");
        }
    }
}
