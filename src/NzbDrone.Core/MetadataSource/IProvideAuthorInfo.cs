using System;
using System.Collections.Generic;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideAuthorInfo
    {
        Author GetAuthorInfo(string readarrId);
        Author GetAuthorAndBooks(string readarrId, double minPopularity = 0);
        HashSet<string> GetChangedArtists(DateTime startTime);
    }
}
