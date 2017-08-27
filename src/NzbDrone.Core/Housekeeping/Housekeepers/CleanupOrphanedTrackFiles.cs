using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedTrackFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedTrackFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM TrackFiles
                                     WHERE Id IN (
                                     SELECT TrackFiles.Id FROM TrackFiles
                                     LEFT OUTER JOIN Tracks
                                     ON TrackFiles.Id = Tracks.TrackFileId
                                     WHERE Tracks.Id IS NULL)");
        }
    }
}
