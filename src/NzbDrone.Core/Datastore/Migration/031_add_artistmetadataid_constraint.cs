using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(31)]
    public class add_artistmetadataid_constraint : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Remove any duplicate artists
            Execute.Sql(@"DELETE FROM Artists
                          WHERE Id NOT IN (
                            SELECT MIN(Artists.id) from Artists
                            JOIN ArtistMetadata ON Artists.ArtistMetadataId = ArtistMetadata.Id
                            GROUP BY ArtistMetadata.Id)");

            // The index exists but will be recreated as part of unique constraint
            Delete.Index().OnTable("Artists").OnColumn("ArtistMetadataId");
            
            // Add a constraint to prevent any more duplicates
            Alter.Column("ArtistMetadataId").OnTable("Artists").AsInt32().Unique();
        }
    }
}
