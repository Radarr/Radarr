using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Update;
using Radarr.Http;

namespace NzbDrone.Api.Update
{
    public class UpdateModule : RadarrRestModule<UpdateResource>
    {
        private readonly IRecentUpdateProvider _recentUpdateProvider;

        public UpdateModule(IRecentUpdateProvider recentUpdateProvider)
        {
            _recentUpdateProvider = recentUpdateProvider;
            GetResourceAll = GetRecentUpdates;
        }

        private List<UpdateResource> GetRecentUpdates()
        {
            var resources = _recentUpdateProvider.GetRecentUpdatePackages()
                                                 .OrderByDescending(u => u.Version)
                                                 .ToResource();

            if (resources.Any())
            {
                var first = resources.First();
                first.Latest = true;

                if (first.Version > BuildInfo.Version)
                {
                    first.Installable = true;
                }

                var installed = resources.SingleOrDefault(r => r.Version == BuildInfo.Version);

                if (installed != null)
                {
                    installed.Installed = true;
                }
            }

            return resources;
        }
    }
}
