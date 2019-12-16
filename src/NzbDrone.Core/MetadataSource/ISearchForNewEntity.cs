using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewEntity
    {
        List<Object> SearchForNewEntity(string title);
    }
}
