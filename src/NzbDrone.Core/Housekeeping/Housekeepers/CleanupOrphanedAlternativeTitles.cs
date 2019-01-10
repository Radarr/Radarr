using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedAlternativeTitles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedAlternativeTitles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.GetDataMapper())
            {

                mapper.ExecuteNonQuery(@"DELETE FROM AlternativeTitles
                                     WHERE Id IN (
                                     SELECT AlternativeTitles.Id FROM AlternativeTitles
                                     LEFT OUTER JOIN Movies
                                     ON AlternativeTitles.MovieId = Movies.Id
                                     WHERE Movies.Id IS NULL)");
            }
        }
    }
}
