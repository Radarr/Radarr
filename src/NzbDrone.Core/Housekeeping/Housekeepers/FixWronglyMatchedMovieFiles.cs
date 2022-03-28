using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class FixWronglyMatchedMovieFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public FixWronglyMatchedMovieFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            /*var mapper = _database.GetDataMapper();

            mapper.Execute(@"UPDATE ""Movies""
                SET ""MovieFileId"" =
                (Select ""Id"" FROM ""MovieFiles"" WHERE ""Movies"".""Id"" == ""MovieFiles"".""MovieId"")
                WHERE ""MovieFileId"" !=
                (SELECT ""Id"" FROM ""MovieFiles"" WHERE ""Movies"".""Id"" == ""MovieFiles"".""MovieId"")");*/
        }
    }
}
