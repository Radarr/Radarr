using System;
using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideAlbumInfo
    {
        Tuple<string, Album, List<ArtistMetadata>> GetAlbumInfo(string id);
        HashSet<string> GetChangedAlbums(DateTime startTime);
    }
}
