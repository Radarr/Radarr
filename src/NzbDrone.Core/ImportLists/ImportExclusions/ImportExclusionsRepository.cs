using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public interface IImportExclusionsRepository : IBasicRepository<ImportExclusion>
    {
        bool IsMovieExcluded(int tmdbid);
        ImportExclusion GetByTmdbid(int tmdbid);
        List<int> AllExcludedTmdbIds();
    }

    public class ImportExclusionsRepository : BasicRepository<ImportExclusion>, IImportExclusionsRepository
    {
        public ImportExclusionsRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool IsMovieExcluded(int tmdbid)
        {
            return Query(x => x.TmdbId == tmdbid).Any();
        }

        public ImportExclusion GetByTmdbid(int tmdbid)
        {
            return Query(x => x.TmdbId == tmdbid).First();
        }

        public List<int> AllExcludedTmdbIds()
        {
            using var conn = _database.OpenConnection();

            return conn.Query<int>("SELECT \"TmdbId\" FROM \"ImportExclusions\"").ToList();
        }
    }
}
