using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.TV
{
    public interface IProvideTVShowInfo
    {
        TVShowMetadata GetTVShowInfo(int tvdbId);
        TVShowMetadata GetTVShowByImdbId(string imdbId);
        TVShowMetadata GetTVShowByTmdbId(int tmdbId);
        List<TVShowMetadata> GetBulkTVShowInfo(List<int> tvdbIds);
        List<SeasonMetadata> GetSeasons(int tvdbId);
        List<EpisodeMetadata> GetEpisodes(int tvdbId, int? seasonNumber = null);
        EpisodeMetadata GetEpisode(int tvdbId, int seasonNumber, int episodeNumber);
        HashSet<int> GetChangedTVShows(DateTime startTime);
    }
}
