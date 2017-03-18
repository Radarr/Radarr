using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedEpisodeFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedEpisodeFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM MovieFiles
                                     WHERE Id IN (
                                     SELECT MovieFiles.Id FROM MovieFiles
                                     LEFT OUTER JOIN Movies
                                     ON MovieFiles.Id = Movies.MovieFileId
                                     WHERE Movies.Id IS NULL)");
        }
    }
}
