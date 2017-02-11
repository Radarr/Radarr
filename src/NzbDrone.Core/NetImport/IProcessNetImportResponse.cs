using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport
{
    public interface IParseNetImportResponse
    {
        IList<Movie> ParseResponse(NetImportResponse netMovieImporterResponse);
    }
}
