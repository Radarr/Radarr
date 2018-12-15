using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;
using NzbDrone.Core.Profiles.Metadata;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideArtistInfo
    {
        Artist GetArtistInfo(string lidarrId, int metadataProfileId);
    }
}
