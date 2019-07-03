using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies.AlternativeTitles
{
    public interface IAlternativeTitleService
    {
        List<AlternativeTitle> GetAllTitlesForMovie(int movieId);
        AlternativeTitle AddAltTitle(AlternativeTitle title, Movie movie);
        List<AlternativeTitle> AddAltTitles(List<AlternativeTitle> titles, Movie movie);
        AlternativeTitle GetById(int id);
        List<AlternativeTitle> GetAllTitles();
        void DeleteNotEnoughVotes(List<AlternativeTitle> mappingsTitles);
    }

    public class AlternativeTitleService : IAlternativeTitleService, IHandleAsync<MovieDeletedEvent>
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

        public List<AlternativeTitle> GetAllTitlesForMovie(int movieId)
        {
            return _titleRepo.FindByMovieId(movieId).ToList();
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

        public List<AlternativeTitle> GetAllTitles()
        {
            return _titleRepo.All().ToList();
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

        public void HandleAsync(MovieDeletedEvent message)
        {
            var title = GetAllTitlesForMovie(message.Movie.Id);
            _titleRepo.DeleteMany(title);

        }
    }
}
