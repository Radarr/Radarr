using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MountCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMovieService _movieService;

        public MountCheck(IDiskProvider diskProvider, IMovieService movieService)
        {
            _diskProvider = diskProvider;
            _movieService = movieService;
        }

        public override HealthCheck Check()
        {
            // Not best for optimization but due to possible symlinks and junctions, we get mounts based on series path so internals can handle mount resolution.
            var mounts = _movieService.AllMoviePaths()
                                      .Select(p => _diskProvider.GetMount(p))
                                      .Where(m => m != null && m.MountOptions != null && m.MountOptions.IsReadOnly)
                                      .DistinctBy(m => m.RootDirectory)
                                      .ToList();

            if (mounts.Any())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "Mount containing a series path is mounted read-only: " + string.Join(",", mounts.Select(m => m.Name)), "#series-mount-ro");
            }

            return new HealthCheck(GetType());
        }
    }
}
