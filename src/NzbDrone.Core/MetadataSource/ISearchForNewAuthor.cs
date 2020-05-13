using System.Collections.Generic;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewAuthor
    {
        List<Author> SearchForNewAuthor(string title);
    }
}
