using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.NetImport.Radarr
{
    public class RadarrParser : IParseNetImportResponse
    {
        private readonly RadarrSettings _settings;
        public RadarrParser(RadarrSettings settings)
        {
            _settings = settings;
        }

        public IList<Movie> ParseResponse(NetImportResponse netMovieImporterResponse)
        {
            var remoteMovies = Json.Deserialize<List<RadarrMovie>>(netMovieImporterResponse.Content);

            var movies = new List<Movie>();

            foreach (var remoteMovie in remoteMovies)
            {
                movies.Add(new Movie
                    {
                        TmdbId = remoteMovie.TmdbId,
                        Title = remoteMovie.Title,
                        SortTitle = remoteMovie.SortTitle,
                        TitleSlug = remoteMovie.TitleSlug,
                        Overview = remoteMovie.Overview,
                        Images = remoteMovie.Images.Select(x => MapImage(x, _settings.BaseUrl)).ToList(),
                        Monitored = remoteMovie.Monitored,
                        PhysicalRelease = remoteMovie.PhysicalRelease,
                        InCinemas = remoteMovie.InCinemas,
                        Year = remoteMovie.Year
                    });
            }

            return movies;
        }

        private static MediaCover.MediaCover MapImage(MediaCover.MediaCover arg, string baseUrl)
        {
            var newImage = new MediaCover.MediaCover
            {
                Url = string.Format("{0}{1}", baseUrl, arg.Url),
                CoverType = arg.CoverType
            };

            return newImage;
        }
    }
}
