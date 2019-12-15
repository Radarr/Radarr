using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(MovieDeletedEvent))]
    [CheckOn(typeof(MovieMovedEvent))]
    public class RootFolderCheck : HealthCheckBase
    {
        private readonly IMovieService _movieService;
        private readonly IDiskProvider _diskProvider;

        public RootFolderCheck(IMovieService movieService, IDiskProvider diskProvider)
        {
            _movieService = movieService;
            _diskProvider = diskProvider;
        }

        public override HealthCheck Check()
        {
            var missingRootFolders = _movieService.AllMoviePaths()
                                                  .Select(s => _diskProvider.GetParentFolder(s))
                                                  .Distinct()
                                                  .Where(s => !_diskProvider.FolderExists(s))
                                                  .ToList();

            if (missingRootFolders.Any())
            {
                if (missingRootFolders.Count == 1)
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Error, "Missing root folder: " + missingRootFolders.First(), "#missing-root-folder");
                }

                var message = string.Format("Multiple root folders are missing: {0}", string.Join(" | ", missingRootFolders));
                return new HealthCheck(GetType(), HealthCheckResult.Error, message, "#missing-root-folder");
            }

            return new HealthCheck(GetType());
        }
    }
}
