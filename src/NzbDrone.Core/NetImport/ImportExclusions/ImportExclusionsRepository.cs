using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.NetImport.ImportExclusions
{
    public interface IImportExclusionsRepository : IBasicRepository<ImportExclusion>
    {
        bool IsMovieExcluded(int tmdbid);
        ImportExclusion GetByTmdbid(int tmdbid);
    }

    public class ImportExclusionsRepository : BasicRepository<ImportExclusion>, IImportExclusionsRepository
    {
        protected IMainDatabase _database;

        public ImportExclusionsRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _database = database;
        }

        public bool IsMovieExcluded(int tmdbid)
        {
            return Query.Where(ex => ex.TmdbId == tmdbid).Any();
        }

        public ImportExclusion GetByTmdbid(int tmdbid)
        {
            return Query.Where(ex => ex.TmdbId == tmdbid).First();
        }
    }
}
