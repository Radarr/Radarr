using System.Collections.Generic;
using System.Linq;
using NLog;
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
        private readonly Logger _logger;

        public AlternativeTitleService(IAlternativeTitleRepository titleRepo, Logger logger)
        {
            _titleRepo = titleRepo;
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
            var movieMetadataId = movieMetadata.Id;

            // First update the movie ids so we can correlate them later.
            titles.ForEach(t => t.MovieMetadataId = movieMetadataId);

            // Then make sure none of them are the same as the main title.
            titles = titles.Where(t => t.CleanTitle != movieMetadata.CleanTitle).ToList();

            // Then make sure they are all distinct titles
            titles = titles.DistinctBy(t => t.CleanTitle).ToList();

            // Make sure we are not adding titles that exist for other movies (until language PR goes in)
            var allTitlesByCleanTitles = _titleRepo.FindByCleanTitles(titles.Select(t => t.CleanTitle).ToList());
            titles = titles.Where(t => !allTitlesByCleanTitles.Any(e => e.CleanTitle == t.CleanTitle && e.MovieMetadataId != t.MovieMetadataId)).ToList();

            var existingTitles = _titleRepo.FindByMovieMetadataId(movieMetadataId);

            var updateList = new List<AlternativeTitle>();
            var addList = new List<AlternativeTitle>();
            var upToDateCount = 0;

            foreach (var title in titles)
            {
                var existingTitle = existingTitles.FirstOrDefault(x => x.CleanTitle == title.CleanTitle);

                if (existingTitle != null)
                {
                    existingTitles.Remove(existingTitle);

                    title.UseDbFieldsFrom(existingTitle);

                    if (!title.Equals(existingTitle))
                    {
                        updateList.Add(title);
                    }
                    else
                    {
                        upToDateCount++;
                    }
                }
                else
                {
                    addList.Add(title);
                }
            }

            _titleRepo.DeleteMany(existingTitles);
            _titleRepo.UpdateMany(updateList);
            _titleRepo.InsertMany(addList);

            _logger.Debug("[{0}] {1} alternative titles up to date; Updating {2}, Adding {3}, Deleting {4} entries.", movieMetadata.Title, upToDateCount, updateList.Count, addList.Count, existingTitles.Count);

            return titles;
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            // TODO handle metadata delete instead of movie delete
            _titleRepo.DeleteForMovies(message.Movies.Select(m => m.MovieMetadataId).ToList());
        }
    }
}
