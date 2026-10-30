using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles
{
    public static class MovieFileVersionExtensions
    {
        // Keeps the files of the version the movie stands for. A version view only gets its own files,
        // the movie itself gets every file that doesn't belong to one of its additional versions.
        public static List<MovieFile> ForVersionOf(this IEnumerable<MovieFile> movieFiles, Movie movie, IEnumerable<int> additionalVersionIds)
        {
            if (movie.IsVersionView)
            {
                return movieFiles.Where(f => f.MovieVersionId == movie.MovieVersionId).ToList();
            }

            return movieFiles.ForPrimaryVersion(additionalVersionIds);
        }

        // The version a file belongs to, 0 for the primary version (including ids of versions that no longer exist)
        public static int GetVersionId(this MovieFile movieFile, IEnumerable<int> additionalVersionIds)
        {
            return additionalVersionIds.Contains(movieFile.MovieVersionId) ? movieFile.MovieVersionId : 0;
        }

        public static List<MovieFile> ForPrimaryVersion(this IEnumerable<MovieFile> movieFiles, IEnumerable<int> additionalVersionIds)
        {
            var additional = additionalVersionIds.ToHashSet();

            return movieFiles.Where(f => !additional.Contains(f.MovieVersionId)).ToList();
        }
    }
}
