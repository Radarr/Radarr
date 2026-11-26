using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MountCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMovieService _movieService;

        public MountCheck(IDiskProvider diskProvider, IMovieService movieService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _diskProvider = diskProvider;
            _movieService = movieService;
        }

        public override HealthCheck Check()
        {
            // Due to possible symlinks and junctions, we get mounts based on movie path so internals can handle mount resolution.
            var mounts = _diskProvider.GetMountsForPaths(_movieService.AllMoviePaths().Values.ToHashSet())
                .Where(m => m.Value is { MountOptions.IsReadOnly: true })
                .DistinctBy(m => m.Value.RootDirectory)
                .ToList();

            if (mounts.Any())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.MountMovies,
                    $"{_localizationService.GetLocalizedString("MountMovieHealthCheckMessage")}{string.Join(", ", mounts.Select(m => $"{m.Value.Name} ({m.Key})"))}",
                    "#movies-mount-ro");
            }

            return new HealthCheck(GetType());
        }
    }
}
