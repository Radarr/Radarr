using System.Collections.Generic;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewSeries
    {
        List<Series> SearchForNewSeries(string title);
        List<Artist> SearchForNewArtist(string title);
    }
}