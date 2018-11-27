using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.NetImport.RadarrInstance
{
    public class RadarrInstanceParser : IParseNetImportResponse
    {
        public IList<Movie> ParseResponse(NetImportResponse netMovieImporterResponse)
        {
            List<Movie> movies =
                Json.Deserialize<List<Movie>>(netMovieImporterResponse.Content);

            return movies;
        }
    }
}
