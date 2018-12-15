using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedAlbums : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedAlbums(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM Albums
                                     WHERE Id IN (
                                     SELECT Albums.Id FROM Albums
                                     LEFT OUTER JOIN Artists
                                     ON Albums.ArtistMetadataId = Artists.ArtistMetadataId
                                     WHERE Artists.Id IS NULL)");
        }
    }
}
