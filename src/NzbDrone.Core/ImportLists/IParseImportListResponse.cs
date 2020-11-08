using System.Collections.Generic;
using NzbDrone.Core.ImportLists.ImportListMovies;

namespace NzbDrone.Core.ImportLists
{
    public interface IParseImportListResponse
    {
        IList<ImportListMovie> ParseResponse(ImportListResponse importListResponse);
    }
}
