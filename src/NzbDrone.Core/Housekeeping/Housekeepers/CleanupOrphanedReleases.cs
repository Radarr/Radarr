using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedReleases : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedReleases(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM AlbumReleases
                                     WHERE Id IN (
                                     SELECT AlbumReleases.Id FROM AlbumReleases
                                     LEFT OUTER JOIN Albums
                                     ON AlbumReleases.AlbumId = Albums.Id
                                     WHERE Albums.Id IS NULL)");
        }
    }
}
