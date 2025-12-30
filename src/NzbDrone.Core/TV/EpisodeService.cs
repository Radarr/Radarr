using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.TV.Events;

namespace NzbDrone.Core.TV
{
    public interface IEpisodeService
    {
        Episode GetEpisode(int episodeId);
        List<Episode> GetEpisodes(IEnumerable<int> episodeIds);
        List<Episode> GetEpisodesByTVShowId(int tvShowId);
        List<Episode> GetEpisodesByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber);
        List<Episode> GetEpisodesBySeasonId(int seasonId);
        Episode GetEpisode(int tvShowId, int seasonNumber, int episodeNumber);
        Episode GetEpisodeByAbsoluteNumber(int tvShowId, int absoluteNumber);
        List<Episode> GetEpisodesByAirDate(int tvShowId, DateTime airDate);
        Episode AddEpisode(Episode newEpisode);
        List<Episode> AddEpisodes(List<Episode> newEpisodes);
        void DeleteEpisode(int episodeId);
        Episode UpdateEpisode(Episode episode);
        List<Episode> UpdateEpisodes(List<Episode> episodes);

        List<Episode> GetEpisodesBySeason(int tvShowId, int seasonNumber);
        Episode FindByAirDate(int tvShowId, string airDate);
        List<Episode> FindByAbsoluteEpisodeNumber(int tvShowId, IEnumerable<int> absoluteNumbers);
        List<Episode> FindBySeasonAndEpisode(int tvShowId, int seasonNumber, IEnumerable<int> episodeNumbers);
    }

    public class EpisodeService : IEpisodeService
    {
        private readonly IEpisodeRepository _episodeRepository;
        private readonly IEventAggregator _eventAggregator;

        public EpisodeService(
            IEpisodeRepository episodeRepository,
            IEventAggregator eventAggregator)
        {
            _episodeRepository = episodeRepository;
            _eventAggregator = eventAggregator;
        }

        public Episode GetEpisode(int episodeId)
        {
            return _episodeRepository.Get(episodeId);
        }

        public List<Episode> GetEpisodes(IEnumerable<int> episodeIds)
        {
            return _episodeRepository.Get(episodeIds).ToList();
        }

        public List<Episode> GetEpisodesByTVShowId(int tvShowId)
        {
            return _episodeRepository.FindByTVShowId(tvShowId);
        }

        public List<Episode> GetEpisodesByTVShowIdAndSeasonNumber(int tvShowId, int seasonNumber)
        {
            return _episodeRepository.FindByTVShowId(tvShowId)
                .Where(e => e.SeasonNumber == seasonNumber)
                .ToList();
        }

        public List<Episode> GetEpisodesBySeasonId(int seasonId)
        {
            return _episodeRepository.FindBySeasonId(seasonId);
        }

        public Episode GetEpisode(int tvShowId, int seasonNumber, int episodeNumber)
        {
            return _episodeRepository.FindByTVShowIdAndEpisodeNumber(tvShowId, seasonNumber, episodeNumber);
        }

        public Episode GetEpisodeByAbsoluteNumber(int tvShowId, int absoluteNumber)
        {
            return _episodeRepository.FindByTVShowIdAndAbsoluteNumber(tvShowId, absoluteNumber);
        }

        public List<Episode> GetEpisodesByAirDate(int tvShowId, DateTime airDate)
        {
            return _episodeRepository.FindByAirDate(tvShowId, airDate);
        }

        public Episode AddEpisode(Episode newEpisode)
        {
            newEpisode.Added = DateTime.UtcNow;
            var episode = _episodeRepository.Insert(newEpisode);
            _eventAggregator.PublishEvent(new EpisodeAddedEvent(episode));
            return episode;
        }

        public List<Episode> AddEpisodes(List<Episode> newEpisodes)
        {
            var now = DateTime.UtcNow;
            foreach (var episode in newEpisodes)
            {
                episode.Added = now;
            }

            _episodeRepository.InsertMany(newEpisodes);

            foreach (var episode in newEpisodes)
            {
                _eventAggregator.PublishEvent(new EpisodeAddedEvent(episode));
            }

            return newEpisodes;
        }

        public void DeleteEpisode(int episodeId)
        {
            var episode = _episodeRepository.Get(episodeId);
            _episodeRepository.Delete(episodeId);
            _eventAggregator.PublishEvent(new EpisodeDeletedEvent(episode));
        }

        public Episode UpdateEpisode(Episode episode)
        {
            var existingEpisode = _episodeRepository.Get(episode.Id);
            var updatedEpisode = _episodeRepository.Update(episode);
            _eventAggregator.PublishEvent(new EpisodeEditedEvent(updatedEpisode, existingEpisode));
            return updatedEpisode;
        }

        public List<Episode> UpdateEpisodes(List<Episode> episodes)
        {
            _episodeRepository.UpdateMany(episodes);
            _eventAggregator.PublishEvent(new EpisodesBulkEditedEvent(episodes));
            return episodes;
        }

        public List<Episode> GetEpisodesBySeason(int tvShowId, int seasonNumber)
        {
            return GetEpisodesByTVShowIdAndSeasonNumber(tvShowId, seasonNumber);
        }

        public Episode FindByAirDate(int tvShowId, string airDate)
        {
            if (!DateTime.TryParse(airDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return null;
            }

            return _episodeRepository.FindByAirDate(tvShowId, parsedDate).FirstOrDefault();
        }

        public List<Episode> FindByAbsoluteEpisodeNumber(int tvShowId, IEnumerable<int> absoluteNumbers)
        {
            var episodes = new List<Episode>();
            foreach (var absNum in absoluteNumbers)
            {
                var episode = _episodeRepository.FindByTVShowIdAndAbsoluteNumber(tvShowId, absNum);
                if (episode != null)
                {
                    episodes.Add(episode);
                }
            }

            return episodes;
        }

        public List<Episode> FindBySeasonAndEpisode(int tvShowId, int seasonNumber, IEnumerable<int> episodeNumbers)
        {
            var episodes = new List<Episode>();
            foreach (var epNum in episodeNumbers)
            {
                var episode = _episodeRepository.FindByTVShowIdAndEpisodeNumber(tvShowId, seasonNumber, epNum);
                if (episode != null)
                {
                    episodes.Add(episode);
                }
            }

            return episodes;
        }
    }
}
