using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(MoviesDeletedEvent))]
    [CheckOn(typeof(MovieMovedEvent))]
    [CheckOn(typeof(MoviesImportedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(MovieImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class RootFolderCheck : HealthCheckBase
    {
        private readonly IMovieService _movieService;
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderService _rootFolderService;

        public RootFolderCheck(IMovieService movieService, IDiskProvider diskProvider, IRootFolderService rootFolderService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _movieService = movieService;
            _diskProvider = diskProvider;
            _rootFolderService = rootFolderService;
        }

        public override HealthCheck Check()
        {
            var rootFolders = _movieService.AllMoviePaths()
                                                           .Select(s => _rootFolderService.GetBestRootFolderPath(s.Value))
                                                           .Distinct();

            var missingRootFolders = rootFolders.Where(s => !_diskProvider.FolderExists(s))
                                                          .ToList();

            if (missingRootFolders.Any())
            {
                if (missingRootFolders.Count == 1)
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("RootFolderCheckSingleMessage"), missingRootFolders.First()), "#missing-root-folder");
                }

                var message = string.Format(_localizationService.GetLocalizedString("RootFolderCheckMultipleMessage"), string.Join(" | ", missingRootFolders));
                return new HealthCheck(GetType(), HealthCheckResult.Error, message, "#missing-root-folder");
            }

            return new HealthCheck(GetType());
        }
    }
}
