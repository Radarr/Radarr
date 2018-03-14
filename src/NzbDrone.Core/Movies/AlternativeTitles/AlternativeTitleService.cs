using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Movies.AlternativeTitles
{
    public interface IAlternativeTitleService
    {
        List<AlternativeTitle> GetAllTitlesForMovie(Movie movie);
        AlternativeTitle AddAltTitle(AlternativeTitle title, Movie movie);
        List<AlternativeTitle> AddAltTitles(List<AlternativeTitle> titles, Movie movie);
        AlternativeTitle GetById(int id);
        void DeleteNotEnoughVotes(List<AlternativeTitle> mappingsTitles);
    }

    public class AlternativeTitleService : IAlternativeTitleService
    {
        private readonly IAlternativeTitleRepository _titleRepo;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;


        public AlternativeTitleService(IAlternativeTitleRepository titleRepo,
                             IEventAggregator eventAggregator,
                             IConfigService configService,
                             Logger logger)
        {
            _titleRepo = titleRepo;
            _eventAggregator = eventAggregator;
            _configService = configService;
            _logger = logger;
        }

        public List<AlternativeTitle> GetAllTitlesForMovie(Movie movie)
        {
            return _titleRepo.All().ToList();
        }

        public AlternativeTitle AddAltTitle(AlternativeTitle title, Movie movie)
        {
            title.MovieId = movie.Id;
            return _titleRepo.Insert(title);
        }

        public List<AlternativeTitle> AddAltTitles(List<AlternativeTitle> titles, Movie movie)
        {
            titles.ForEach(t => t.MovieId = movie.Id);
            _titleRepo.InsertMany(titles);
            return titles;
        }

        public AlternativeTitle GetById(int id)
        {
            return _titleRepo.Get(id);
        }

        public void RemoveTitle(AlternativeTitle title)
        {
            _titleRepo.Delete(title);
        }

        public void DeleteNotEnoughVotes(List<AlternativeTitle> mappingsTitles)
        {
            var toRemove = mappingsTitles.Where(t => t.SourceType == SourceType.Mappings && t.Votes < 4);
            var realT = _titleRepo.FindBySourceIds(toRemove.Select(t => t.SourceId).ToList());
            _titleRepo.DeleteMany(realT);
        }
    }
}
