using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.MetadataSource.TV
{
    public class TVDbProxy : IProvideTVShowInfo, ISearchForNewTVShow
    {
        private const string BaseUrl = "https://api4.thetvdb.com/v4";

        private readonly IHttpClient _httpClient;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public TVDbProxy(
            IHttpClient httpClient,
            IConfigService configService,
            Logger logger)
        {
            _httpClient = httpClient;
            _configService = configService;
            _logger = logger;
        }

        public TVShowMetadata GetTVShowInfo(int tvdbId)
        {
            _logger.Debug("Getting TV show info for TVDb ID: {0}", tvdbId);

            // TODO: Implement actual TVDb API call
            // For now, return null - actual implementation will make HTTP request
            // to TVDb API v4 endpoint: GET /series/{id}/extended
            throw new NotImplementedException("TVDb API integration not yet implemented");
        }

        public TVShowMetadata GetTVShowByImdbId(string imdbId)
        {
            _logger.Debug("Getting TV show info for IMDb ID: {0}", imdbId);
            throw new NotImplementedException("TVDb API integration not yet implemented");
        }

        public TVShowMetadata GetTVShowByTmdbId(int tmdbId)
        {
            _logger.Debug("Getting TV show info for TMDb ID: {0}", tmdbId);
            throw new NotImplementedException("TVDb API integration not yet implemented");
        }

        public List<TVShowMetadata> GetBulkTVShowInfo(List<int> tvdbIds)
        {
            _logger.Debug("Getting bulk TV show info for {0} shows", tvdbIds.Count);
            throw new NotImplementedException("TVDb API integration not yet implemented");
        }

        public List<SeasonMetadata> GetSeasons(int tvdbId)
        {
            _logger.Debug("Getting seasons for TVDb ID: {0}", tvdbId);
            throw new NotImplementedException("TVDb API integration not yet implemented");
        }

        public List<EpisodeMetadata> GetEpisodes(int tvdbId, int? seasonNumber = null)
        {
            _logger.Debug("Getting episodes for TVDb ID: {0}, Season: {1}", tvdbId, seasonNumber?.ToString() ?? "all");
            throw new NotImplementedException("TVDb API integration not yet implemented");
        }

        public EpisodeMetadata GetEpisode(int tvdbId, int seasonNumber, int episodeNumber)
        {
            _logger.Debug("Getting episode TVDb ID: {0}, S{1}E{2}", tvdbId, seasonNumber, episodeNumber);
            throw new NotImplementedException("TVDb API integration not yet implemented");
        }

        public HashSet<int> GetChangedTVShows(DateTime startTime)
        {
            _logger.Debug("Getting changed TV shows since: {0}", startTime);
            throw new NotImplementedException("TVDb API integration not yet implemented");
        }

        public List<TVShowMetadata> SearchForNewTVShow(string title)
        {
            _logger.Debug("Searching for TV show: {0}", title);
            throw new NotImplementedException("TVDb API integration not yet implemented");
        }

        public List<TVShowMetadata> SearchByTvdbId(int tvdbId)
        {
            _logger.Debug("Searching by TVDb ID: {0}", tvdbId);
            var result = GetTVShowInfo(tvdbId);
            return result != null ? new List<TVShowMetadata> { result } : new List<TVShowMetadata>();
        }

        public List<TVShowMetadata> SearchByImdbId(string imdbId)
        {
            _logger.Debug("Searching by IMDb ID: {0}", imdbId);
            var result = GetTVShowByImdbId(imdbId);
            return result != null ? new List<TVShowMetadata> { result } : new List<TVShowMetadata>();
        }

        public List<TVShowMetadata> SearchByTmdbId(int tmdbId)
        {
            _logger.Debug("Searching by TMDb ID: {0}", tmdbId);
            var result = GetTVShowByTmdbId(tmdbId);
            return result != null ? new List<TVShowMetadata> { result } : new List<TVShowMetadata>();
        }

        public List<TVShowMetadata> GetTrendingTVShows()
        {
            _logger.Debug("Getting trending TV shows");
            throw new NotImplementedException("TVDb API integration not yet implemented");
        }

        public List<TVShowMetadata> GetPopularTVShows()
        {
            _logger.Debug("Getting popular TV shows");
            throw new NotImplementedException("TVDb API integration not yet implemented");
        }
    }
}
