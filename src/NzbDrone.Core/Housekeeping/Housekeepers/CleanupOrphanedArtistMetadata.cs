using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedArtistMetadata : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedArtistMetadata(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM ArtistMetadata
                                     WHERE Id IN (
                                     SELECT ArtistMetadata.Id FROM ArtistMetadata
                                     LEFT OUTER JOIN Albums ON Albums.ArtistMetadataId = ArtistMetadata.Id
                                     LEFT OUTER JOIN Tracks ON Tracks.ArtistMetadataId = ArtistMetadata.Id
                                     LEFT OUTER JOIN Artists ON Artists.ArtistMetadataId = ArtistMetadata.Id
                                     WHERE Albums.Id IS NULL AND Tracks.Id IS NULL AND Artists.Id IS NULL)");
        }
    }
}
