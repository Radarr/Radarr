using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedTracks : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedTracks(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM Tracks
                                     WHERE Id IN (
                                     SELECT Tracks.Id FROM Tracks
                                     LEFT OUTER JOIN Albums
                                     ON Tracks.AlbumId = Albums.Id
                                     WHERE Albums.Id IS NULL)");
        }
    }
}
