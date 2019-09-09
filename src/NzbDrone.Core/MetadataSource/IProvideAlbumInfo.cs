using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideAlbumInfo
    {
        Tuple<string, Album, List<ArtistMetadata>> GetAlbumInfo(string id);
    }
}
