using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.NetImport.RadarrInstance
{
    public class RadarrInstanceParser : IParseNetImportResponse
    {
        private RadarrInstanceSettings _settings;
        public RadarrInstanceParser(RadarrInstanceSettings settings)
        {
            _settings = settings;
        }
        public IList<Movie> ParseResponse(NetImportResponse netMovieImporterResponse)
        {
            List<Movie> movies =
                Json.Deserialize<List<Movie>>(netMovieImporterResponse.Content);

            if (_settings.ProfileId >= 0)
            {
                movies = movies.Where(m => m.ProfileId == _settings.ProfileId).ToList();
            }

            return movies;
        }
    }
}
