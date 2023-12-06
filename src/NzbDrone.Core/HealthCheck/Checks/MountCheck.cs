using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MountCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        public MountCheck(IDiskProvider diskProvider, IMovieService movieService, ILocalizationService localizationService, Logger logger)
            : base(localizationService)
        {
            _diskProvider = diskProvider;
            _movieService = movieService;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            _logger.Debug("[MountCheck] Total movie paths: {0}",  _movieService.AllMoviePaths().Count);

            // Not best for optimization but due to possible symlinks and junctions, we get mounts based on series path so internals can handle mount resolution.
            var mounts = _movieService.AllMoviePaths()
                .Select(p => _diskProvider.GetMount(p.Value))
                .Where(m => m is { MountOptions.IsReadOnly: true })
                .DistinctBy(m => m.RootDirectory)
                .ToList();

            if (mounts.Any())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, _localizationService.GetLocalizedString("MountCheckMessage") + string.Join(", ", mounts.Select(m => m.Name)), "#movie-mount-ro");
            }

            return new HealthCheck(GetType());
        }
    }
}
