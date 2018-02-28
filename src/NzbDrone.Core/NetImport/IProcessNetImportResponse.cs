using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.NetImport
{
    public interface IParseNetImportResponse
    {
        IList<Movie> ParseResponse(NetImportResponse netMovieImporterResponse);
    }
}
