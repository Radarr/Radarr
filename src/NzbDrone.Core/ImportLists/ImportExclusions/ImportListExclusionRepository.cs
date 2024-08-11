using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public interface IImportListExclusionRepository : IBasicRepository<ImportListExclusion>
    {
        bool IsMovieExcluded(int tmdbid);
        ImportListExclusion FindByTmdbid(int tmdbid);
        List<int> AllExcludedTmdbIds();
    }

    public class ImportListListExclusionRepository : BasicRepository<ImportListExclusion>, IImportListExclusionRepository
    {
        public ImportListListExclusionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool IsMovieExcluded(int tmdbid)
        {
            return Query(x => x.TmdbId == tmdbid).Any();
        }

        public ImportListExclusion FindByTmdbid(int tmdbid)
        {
            return Query(x => x.TmdbId == tmdbid).SingleOrDefault();
        }

        public List<int> AllExcludedTmdbIds()
        {
            using var conn = _database.OpenConnection();

            return conn.Query<int>("SELECT \"TmdbId\" FROM \"ImportExclusions\"").ToList();
        }
    }
}
