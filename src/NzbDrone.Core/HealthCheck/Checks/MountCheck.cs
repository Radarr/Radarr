using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MountCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IAuthorService _authorService;

        public MountCheck(IDiskProvider diskProvider, IAuthorService authorService)
        {
            _diskProvider = diskProvider;
            _authorService = authorService;
        }

        public override HealthCheck Check()
        {
            // Not best for optimization but due to possible symlinks and junctions, we get mounts based on series path so internals can handle mount resolution.
            var mounts = _authorService.AllAuthorPaths()
                                       .Select(path => _diskProvider.GetMount(path.Value))
                                       .Where(m => m != null && m.MountOptions != null && m.MountOptions.IsReadOnly)
                                       .DistinctBy(m => m.RootDirectory)
                                       .ToList();

            if (mounts.Any())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "Mount containing a author path is mounted read-only: " + string.Join(",", mounts.Select(m => m.Name)), "#author-mount-ro");
            }

            return new HealthCheck(GetType());
        }
    }
}
