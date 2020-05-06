using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewAuthor
    {
        List<Author> SearchForNewAuthor(string title);
    }
}
