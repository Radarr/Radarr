using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MountCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IArtistService _artistService;

        public MountCheck(IDiskProvider diskProvider, IArtistService artistService)
        {
            _diskProvider = diskProvider;
            _artistService = artistService;
        }

        public override HealthCheck Check()
        {
            // Not best for optimization but due to possible symlinks and junctions, we get mounts based on series path so internals can handle mount resolution.
            var mounts = _artistService.GetAllArtists()
                                       .Select(artist => _diskProvider.GetMount(artist.Path))
                                       .Where(m => m != null && m.MountOptions != null && m.MountOptions.IsReadOnly)
                                       .DistinctBy(m => m.RootDirectory)
                                       .ToList();

            if (mounts.Any())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "Mount containing a artist path is mounted read-only: " + string.Join(",", mounts.Select(m => m.Name)), "#artist-mount-ro");
            }

            return new HealthCheck(GetType());
        }
    }
}
