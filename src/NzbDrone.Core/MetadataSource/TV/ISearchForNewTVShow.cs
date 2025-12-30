using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.TV
{
    public interface ISearchForNewTVShow
    {
        List<TVShowMetadata> SearchForNewTVShow(string title);
        List<TVShowMetadata> SearchByTvdbId(int tvdbId);
        List<TVShowMetadata> SearchByImdbId(string imdbId);
        List<TVShowMetadata> SearchByTmdbId(int tmdbId);
        List<TVShowMetadata> GetTrendingTVShows();
        List<TVShowMetadata> GetPopularTVShows();
    }
}
