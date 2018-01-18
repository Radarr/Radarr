using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideAlbumInfo
    {
        Tuple<Album, List<Track>> GetAlbumInfo(string lidarrId, string releaseId);
    }
}
