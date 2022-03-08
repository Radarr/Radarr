using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies.AlternativeTitles
{
    public interface IAlternativeTitleService
    {
        List<AlternativeTitle> GetAllTitlesForMovieMetadata(int movieMetadataId);
        AlternativeTitle AddAltTitle(AlternativeTitle title, MovieMetadata movie);
        List<AlternativeTitle> AddAltTitles(List<AlternativeTitle> titles, MovieMetadata movie);
        AlternativeTitle GetById(int id);
        List<AlternativeTitle> GetAllTitles();
        List<AlternativeTitle> UpdateTitles(List<AlternativeTitle> titles, MovieMetadata movie);
    }

    public class AlternativeTitleService : IAlternativeTitleService, IHandleAsync<MoviesDeletedEvent>
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

        public List<AlternativeTitle> GetAllTitlesForMovieMetadata(int movieMetadataId)
        {
            return _titleRepo.FindByMovieMetadataId(movieMetadataId).ToList();
        }

        public AlternativeTitle AddAltTitle(AlternativeTitle title, MovieMetadata movie)
        {
            title.MovieMetadataId = movie.Id;
            return _titleRepo.Insert(title);
        }

        public List<AlternativeTitle> AddAltTitles(List<AlternativeTitle> titles, MovieMetadata movie)
        {
            titles.ForEach(t => t.MovieMetadataId = movie.Id);
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

        public List<AlternativeTitle> UpdateTitles(List<AlternativeTitle> titles, MovieMetadata movieMetadata)
        {
            int movieMetadataId = movieMetadata.Id;

            // First update the movie ids so we can correlate them later.
            titles.ForEach(t => t.MovieMetadataId = movieMetadataId);

            // Then make sure none of them are the same as the main title.
            titles = titles.Where(t => t.CleanTitle != movieMetadata.CleanTitle).ToList();

            // Then make sure they are all distinct titles
            titles = titles.DistinctBy(t => t.CleanTitle).ToList();

            // Make sure we are not adding titles that exist for other movies (until language PR goes in)
            titles = titles.Where(t => !_titleRepo.All().Any(e => e.CleanTitle == t.CleanTitle && e.MovieMetadataId != t.MovieMetadataId)).ToList();

            // Now find titles to delete, update and insert.
            var existingTitles = _titleRepo.FindByMovieMetadataId(movieMetadataId);

            var insert = titles.Where(t => !existingTitles.Contains(t));
            var update = existingTitles.Where(t => titles.Contains(t));
            var delete = existingTitles.Where(t => !titles.Contains(t));

            _titleRepo.DeleteMany(delete.ToList());
            _titleRepo.UpdateMany(update.ToList());
            _titleRepo.InsertMany(insert.ToList());

            return titles;
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            // TODO hanlde metadata delete instead of movie delete
            _titleRepo.DeleteForMovies(message.Movies.Select(m => m.MovieMetadataId).ToList());
        }
    }
}
