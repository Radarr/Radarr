using System;
using System.Collections.Generic;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideBookInfo
    {
        Tuple<string, Book, List<AuthorMetadata>> GetBookInfo(string id);
        HashSet<string> GetChangedBooks(DateTime startTime);
    }
}
