using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport.ListMovies;

namespace NzbDrone.Core.NetImport
{
    public interface IParseNetImportResponse
    {
        IList<ListMovie> ParseResponse(NetImportResponse netMovieImporterResponse);
    }
}
