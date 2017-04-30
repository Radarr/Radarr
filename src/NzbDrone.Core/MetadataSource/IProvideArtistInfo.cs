using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public interface IProvideArtistInfo
    {
        Tuple<Artist, List<Track>> GetArtistInfo(int itunesId);
    }
}
