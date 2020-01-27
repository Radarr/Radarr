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
    }
}
