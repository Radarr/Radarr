using System;
using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewAlbum
    {
        List<Album> SearchForNewAlbum(string title, string artist);
    }
}
