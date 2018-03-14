using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.NetImport.ImportExclusions
{
    public interface IImportExclusionsService
    {
        List<ImportExclusion> GetAllExclusions();
        bool IsMovieExcluded(int tmdbid);
        ImportExclusion AddExclusion(ImportExclusion exclusion);
        void RemoveExclusion(ImportExclusion exclusion);
        ImportExclusion GetById(int id);
    }

    public class ImportExclusionsService : IImportExclusionsService
    {
        private readonly IImportExclusionsRepository _exclusionRepository;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;


        public ImportExclusionsService(IImportExclusionsRepository exclusionRepository,
                             IEventAggregator eventAggregator,
                             IConfigService configService,
                             Logger logger)
        {
            _exclusionRepository = exclusionRepository;
            _eventAggregator = eventAggregator;
            _configService = configService;
            _logger = logger;
        }

        public ImportExclusion AddExclusion(ImportExclusion exclusion)
        {
            if (_exclusionRepository.IsMovieExcluded(exclusion.TmdbId))
            {
                return _exclusionRepository.GetByTmdbid(exclusion.TmdbId);
            }
            return _exclusionRepository.Insert(exclusion);
        }

        public List<ImportExclusion> GetAllExclusions()
        {
            return _exclusionRepository.All().ToList();
        }

        public bool IsMovieExcluded(int tmdbid)
        {
            return _exclusionRepository.IsMovieExcluded(tmdbid);
        }

        public void RemoveExclusion(ImportExclusion exclusion)
        {
            _exclusionRepository.Delete(exclusion);
        }

        public ImportExclusion GetById(int id)
        {
            return _exclusionRepository.Get(id);
        }
    }
}
