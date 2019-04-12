using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Datastore.Extensions;
using Marr.Data.QGen;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.RomanNumerals;
using NzbDrone.Core.Qualities;
using CoreParser = NzbDrone.Core.Parser.Parser;

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
            return Query(q => q.Where(ex => ex.TmdbId == tmdbid).Any());
        }

        public ImportExclusion GetByTmdbid(int tmdbid)
        {
            return Query(q => q.Where(ex => ex.TmdbId == tmdbid).First());
        }
    }
}
