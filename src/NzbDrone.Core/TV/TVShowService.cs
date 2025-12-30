using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Monitoring;
using NzbDrone.Core.TV.Events;

namespace NzbDrone.Core.TV
{
    public interface ITVShowService
    {
        TVShow GetTVShow(int tvShowId);
        List<TVShow> GetTVShows(IEnumerable<int> tvShowIds);
        TVShow AddTVShow(TVShow newTVShow);
        List<TVShow> AddTVShows(List<TVShow> newTVShows);
        TVShow FindByTitle(string title);
        TVShow FindByTvdbId(int tvdbId);
        TVShow FindByImdbId(string imdbId);
        void DeleteTVShow(int tvShowId, bool deleteFiles);
        void DeleteTVShows(List<int> tvShowIds, bool deleteFiles);
        List<TVShow> GetAllTVShows();
        List<TVShow> GetMonitoredTVShows();
        TVShow UpdateTVShow(TVShow tvShow);
        List<TVShow> UpdateTVShows(List<TVShow> tvShows);
        bool TVShowPathExists(string path);
    }

    public class TVShowService : ITVShowService
    {
        private readonly ITVShowRepository _tvShowRepository;
        private readonly IHierarchicalMonitoringService _hierarchicalMonitoringService;
        private readonly IEventAggregator _eventAggregator;

        public TVShowService(
            ITVShowRepository tvShowRepository,
            IHierarchicalMonitoringService hierarchicalMonitoringService,
            IEventAggregator eventAggregator)
        {
            _tvShowRepository = tvShowRepository;
            _hierarchicalMonitoringService = hierarchicalMonitoringService;
            _eventAggregator = eventAggregator;
        }

        public TVShow GetTVShow(int tvShowId)
        {
            return _tvShowRepository.Get(tvShowId);
        }

        public List<TVShow> GetTVShows(IEnumerable<int> tvShowIds)
        {
            return _tvShowRepository.Get(tvShowIds).ToList();
        }

        public TVShow AddTVShow(TVShow newTVShow)
        {
            newTVShow.Added = DateTime.UtcNow;
            var tvShow = _tvShowRepository.Insert(newTVShow);
            _eventAggregator.PublishEvent(new TVShowAddedEvent(tvShow));
            return tvShow;
        }

        public List<TVShow> AddTVShows(List<TVShow> newTVShows)
        {
            var now = DateTime.UtcNow;
            foreach (var tvShow in newTVShows)
            {
                tvShow.Added = now;
            }

            _tvShowRepository.InsertMany(newTVShows);

            foreach (var tvShow in newTVShows)
            {
                _eventAggregator.PublishEvent(new TVShowAddedEvent(tvShow));
            }

            return newTVShows;
        }

        public TVShow FindByTitle(string title)
        {
            return _tvShowRepository.FindByTitle(title);
        }

        public TVShow FindByTvdbId(int tvdbId)
        {
            return _tvShowRepository.FindByTvdbId(tvdbId);
        }

        public TVShow FindByImdbId(string imdbId)
        {
            return _tvShowRepository.FindByImdbId(imdbId);
        }

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

        public List<TVShow> GetAllTVShows()
        {
            return _tvShowRepository.All().ToList();
        }

        public List<TVShow> GetMonitoredTVShows()
        {
            return _tvShowRepository.GetMonitored();
        }

        public TVShow UpdateTVShow(TVShow tvShow)
        {
            var existingTVShow = _tvShowRepository.Get(tvShow.Id);

            if (existingTVShow.Monitored != tvShow.Monitored)
            {
                _hierarchicalMonitoringService.SetTVShowMonitored(tvShow.Id, tvShow.Monitored);
            }

            var updatedTVShow = _tvShowRepository.Update(tvShow);
            _eventAggregator.PublishEvent(new TVShowEditedEvent(updatedTVShow, existingTVShow));
            return updatedTVShow;
        }

        public List<TVShow> UpdateTVShows(List<TVShow> tvShows)
        {
            _tvShowRepository.UpdateMany(tvShows);
            return tvShows;
        }

        public bool TVShowPathExists(string path)
        {
            return _tvShowRepository.TVShowPathExists(path);
        }
    }
}
