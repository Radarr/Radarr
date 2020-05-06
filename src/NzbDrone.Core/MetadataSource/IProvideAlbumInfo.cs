using System;
using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideBookInfo
    {
        Tuple<string, Book, List<AuthorMetadata>> GetBookInfo(string id);
        HashSet<string> GetChangedAlbums(DateTime startTime);
    }
}
