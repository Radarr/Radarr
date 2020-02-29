using System;
using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideArtistInfo
    {
        Artist GetArtistInfo(string readarrId, int metadataProfileId);
        HashSet<string> GetChangedArtists(DateTime startTime);
    }
}
