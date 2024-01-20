using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public class AggregateEdition : IAggregateLocalMovie
    {
        public int Order => 1;

        public LocalMovie Aggregate(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var movieEdition = localMovie.DownloadClientMovieInfo?.Edition;

            if (movieEdition.IsNullOrWhiteSpace())
            {
                movieEdition = localMovie.FolderMovieInfo?.Edition;
            }

            if (movieEdition.IsNullOrWhiteSpace())
            {
                movieEdition = localMovie.FileMovieInfo?.Edition;
            }

            localMovie.Edition = movieEdition;

            return localMovie;
        }
    }
}
