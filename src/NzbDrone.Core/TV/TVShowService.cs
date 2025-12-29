using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.TV.Events;

namespace NzbDrone.Core.TV
{
    public interface ITVShowService
    {
        TVShow GetTVShow(int tvShowId);
        List<TVShow> GetTVShows(IEnumerable<int> tvShowIds);
        TVShow AddTVShow(TVShow newTVShow);
        List<TVShow> AddTVShows(List<TVShow> newTVShows);
        TVShow FindByTvdbId(int tvdbId);
        TVShow FindByImdbId(string imdbId);
        TVShow FindByAniDbId(int aniDbId);
        TVShow FindByTitle(string title);
        TVShow FindByPath(string path);
        List<TVShow> GetMonitored();
        Dictionary<int, string> AllTVShowPaths();
        void DeleteTVShow(int tvShowId, bool deleteFiles);
        void DeleteTVShows(List<int> tvShowIds, bool deleteFiles);
        List<TVShow> GetAllTVShows();
        Dictionary<int, List<int>> AllTVShowTags();
        TVShow UpdateTVShow(TVShow tvShow);
        List<TVShow> UpdateTVShows(List<TVShow> tvShows);
        bool TVShowPathExists(string folder);
    }

    public class TVShowService : ITVShowService
    {
        private readonly ITVShowRepository _tvShowRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public TVShowService(ITVShowRepository tvShowRepository,
                             IEventAggregator eventAggregator,
                             Logger logger)
        {
            _tvShowRepository = tvShowRepository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public TVShow GetTVShow(int tvShowId) => _tvShowRepository.Get(tvShowId);
        public List<TVShow> GetTVShows(IEnumerable<int> tvShowIds) => _tvShowRepository.Get(tvShowIds).ToList();

        public TVShow AddTVShow(TVShow newTVShow)
        {
            var tvShow = _tvShowRepository.Insert(newTVShow);
            _eventAggregator.PublishEvent(new TVShowAddedEvent(tvShow));
            return tvShow;
        }

        public List<TVShow> AddTVShows(List<TVShow> newTVShows)
        {
            _tvShowRepository.InsertMany(newTVShows);

            foreach (var tvShow in newTVShows)
            {
                _eventAggregator.PublishEvent(new TVShowAddedEvent(tvShow));
            }

            return newTVShows;
        }

        public TVShow FindByTvdbId(int tvdbId) => _tvShowRepository.FindByTvdbId(tvdbId);
        public TVShow FindByImdbId(string imdbId) => _tvShowRepository.FindByImdbId(imdbId);
        public TVShow FindByAniDbId(int aniDbId) => _tvShowRepository.FindByAniDbId(aniDbId);
        public TVShow FindByTitle(string title) => _tvShowRepository.FindByTitle(title);
        public TVShow FindByPath(string path) => _tvShowRepository.FindByPath(path);
        public List<TVShow> GetMonitored() => _tvShowRepository.GetMonitored();
        public Dictionary<int, string> AllTVShowPaths() => _tvShowRepository.AllTVShowPaths();
        public Dictionary<int, List<int>> AllTVShowTags() => _tvShowRepository.AllTVShowTags();
        public bool TVShowPathExists(string folder) => _tvShowRepository.TVShowPathExists(folder);
        public List<TVShow> GetAllTVShows() => _tvShowRepository.All().ToList();

        public void DeleteTVShow(int tvShowId, bool deleteFiles)
        {
            var tvShow = _tvShowRepository.Get(tvShowId);
            _tvShowRepository.Delete(tvShowId);
            _eventAggregator.PublishEvent(new TVShowDeletedEvent(tvShow, deleteFiles));
        }

        public void DeleteTVShows(List<int> tvShowIds, bool deleteFiles)
        {
            var tvShows = _tvShowRepository.Get(tvShowIds).ToList();
            _tvShowRepository.DeleteMany(tvShowIds);

            foreach (var tvShow in tvShows)
            {
                _eventAggregator.PublishEvent(new TVShowDeletedEvent(tvShow, deleteFiles));
            }
        }

        public TVShow UpdateTVShow(TVShow tvShow)
        {
            var storedTVShow = _tvShowRepository.Get(tvShow.Id);
            _tvShowRepository.Update(tvShow);
            _eventAggregator.PublishEvent(new TVShowEditedEvent(tvShow, storedTVShow));
            return tvShow;
        }

        public List<TVShow> UpdateTVShows(List<TVShow> tvShows)
        {
            _tvShowRepository.UpdateMany(tvShows);
            _eventAggregator.PublishEvent(new TVShowsBulkEditedEvent(tvShows));
            return tvShows;
        }
    }
}
