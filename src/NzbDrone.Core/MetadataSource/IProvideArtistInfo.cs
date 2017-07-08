using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideArtistInfo
    {
        Tuple<Artist, List<Album>> GetArtistInfo(string lidarrId);
    }
}
