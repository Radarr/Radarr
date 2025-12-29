using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.TV.Events;

namespace NzbDrone.Core.TV
{
    public interface IEpisodeService
    {
        Episode GetEpisode(int episodeId);
        List<Episode> GetEpisodes(IEnumerable<int> episodeIds);
        List<Episode> GetEpisodesByTVShowId(int tvShowId);
        List<Episode> GetEpisodesBySeasonId(int seasonId);
        List<Episode> GetEpisodesByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber);
        Episode FindByTVShowIdAndEpisode(int tvShowId, int seasonNumber, int episodeNumber);
        Episode FindByTVShowIdAndAbsoluteNumber(int tvShowId, int absoluteNumber);
        Episode AddEpisode(Episode newEpisode);
        List<Episode> AddEpisodes(List<Episode> newEpisodes);
        void DeleteEpisode(int episodeId, bool deleteFiles);
        void DeleteEpisodes(List<int> episodeIds, bool deleteFiles);
        Episode UpdateEpisode(Episode episode);
        List<Episode> UpdateEpisodes(List<Episode> episodes);
        List<Episode> GetEpisodesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        Episode FindByPath(string path);
        Dictionary<int, string> AllEpisodePaths();
    }

    public class EpisodeService : IEpisodeService
    {
        private readonly IEpisodeRepository _episodeRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public EpisodeService(IEpisodeRepository episodeRepository,
                              IEventAggregator eventAggregator,
                              Logger logger)
        {
            _episodeRepository = episodeRepository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public Episode GetEpisode(int episodeId) => _episodeRepository.Get(episodeId);
        public List<Episode> GetEpisodes(IEnumerable<int> episodeIds) => _episodeRepository.Get(episodeIds).ToList();
        public List<Episode> GetEpisodesByTVShowId(int tvShowId) => _episodeRepository.FindByTVShowId(tvShowId);
        public List<Episode> GetEpisodesBySeasonId(int seasonId) => _episodeRepository.FindBySeasonId(seasonId);
        public List<Episode> GetEpisodesByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber)
            => _episodeRepository.FindByTVShowIdAndSeasonNumber(tvShowId, seasonNumber);
        public Episode FindByTVShowIdAndEpisode(int tvShowId, int seasonNumber, int episodeNumber)
            => _episodeRepository.FindByTVShowIdAndEpisode(tvShowId, seasonNumber, episodeNumber);
        public Episode FindByTVShowIdAndAbsoluteNumber(int tvShowId, int absoluteNumber)
            => _episodeRepository.FindByTVShowIdAndAbsoluteNumber(tvShowId, absoluteNumber);
        public Episode FindByPath(string path) => _episodeRepository.FindByPath(path);
        public Dictionary<int, string> AllEpisodePaths() => _episodeRepository.AllEpisodePaths();
        public List<Episode> GetEpisodesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
            => _episodeRepository.EpisodesBetweenDates(start, end, includeUnmonitored);

        public Episode AddEpisode(Episode newEpisode)
        {
            var episode = _episodeRepository.Insert(newEpisode);
            _eventAggregator.PublishEvent(new EpisodeAddedEvent(episode));
            return episode;
        }

        public List<Episode> AddEpisodes(List<Episode> newEpisodes)
        {
            _episodeRepository.InsertMany(newEpisodes);

            foreach (var episode in newEpisodes)
            {
                _eventAggregator.PublishEvent(new EpisodeAddedEvent(episode));
            }

            return newEpisodes;
        }

        public void DeleteEpisode(int episodeId, bool deleteFiles)
        {
            var episode = _episodeRepository.Get(episodeId);
            _episodeRepository.Delete(episodeId);
            _eventAggregator.PublishEvent(new EpisodeDeletedEvent(episode, deleteFiles));
        }

        public void DeleteEpisodes(List<int> episodeIds, bool deleteFiles)
        {
            var episodes = _episodeRepository.Get(episodeIds).ToList();
            _episodeRepository.DeleteMany(episodeIds);

            foreach (var episode in episodes)
            {
                _eventAggregator.PublishEvent(new EpisodeDeletedEvent(episode, deleteFiles));
            }
        }

        public Episode UpdateEpisode(Episode episode)
        {
            var storedEpisode = _episodeRepository.Get(episode.Id);
            _episodeRepository.Update(episode);
            _eventAggregator.PublishEvent(new EpisodeEditedEvent(episode, storedEpisode));
            return episode;
        }

        public List<Episode> UpdateEpisodes(List<Episode> episodes)
        {
            _episodeRepository.UpdateMany(episodes);
            _eventAggregator.PublishEvent(new EpisodesBulkEditedEvent(episodes));
            return episodes;
        }
    }
}
