using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewArtist
    {
        List<Artist> SearchForNewArtist(string title);
    }
}