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

            // Delete where track no longer exists
            mapper.ExecuteNonQuery(@"DELETE FROM TrackFiles
                                     WHERE Id IN (
                                     SELECT TrackFiles.Id FROM TrackFiles
                                     LEFT OUTER JOIN Tracks
                                     ON TrackFiles.Id = Tracks.TrackFileId
                                     WHERE Tracks.Id IS NULL)");

            // Delete trackfiles associated with releases that are not currently selected
            mapper.ExecuteNonQuery(@"DELETE FROM TrackFiles
                                     WHERE Id IN (
                                     SELECT TrackFiles.Id FROM TrackFiles
                                     JOIN Tracks ON TrackFiles.Id = Tracks.TrackFileId
                                     JOIN AlbumReleases ON Tracks.AlbumReleaseId = AlbumReleases.Id
                                     JOIN Albums ON AlbumReleases.AlbumId = Albums.Id
                                     WHERE AlbumReleases.Monitored = 0)");

            // Unlink Tracks where the Trackfiles entry no longer exists
            mapper.ExecuteNonQuery(@"UPDATE Tracks
                                     SET TrackFileId = 0
                                     WHERE Id IN (
                                     SELECT Tracks.Id FROM Tracks
                                     LEFT OUTER JOIN TrackFiles
                                     ON Tracks.TrackFileId = TrackFiles.Id
                                     WHERE TrackFiles.Id IS NULL)");
        }
    }
}
