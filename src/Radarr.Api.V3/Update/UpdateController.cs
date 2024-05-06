using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Update;
using NzbDrone.Core.Update.History;
using Radarr.Http;

namespace Radarr.Api.V3.Update
{
    [V3ApiController]
    public class UpdateController : Controller
    {
        private readonly IRecentUpdateProvider _recentUpdateProvider;
        private readonly IUpdateHistoryService _updateHistoryService;
        private readonly IConfigFileProvider _configFileProvider;

        public UpdateController(IRecentUpdateProvider recentUpdateProvider, IUpdateHistoryService updateHistoryService, IConfigFileProvider configFileProvider)
        {
            _recentUpdateProvider = recentUpdateProvider;
            _updateHistoryService = updateHistoryService;
            _configFileProvider = configFileProvider;
        }

        [HttpGet]
        public List<UpdateResource> GetRecentUpdates()
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

                if (!_configFileProvider.LogDbEnabled)
                {
                    return resources;
                }

                var installDates = _updateHistoryService.InstalledSince(resources.Last().ReleaseDate)
                                                        .DistinctBy(v => v.Version)
                                                        .ToDictionary(v => v.Version);

                foreach (var resource in resources)
                {
                    if (installDates.TryGetValue(resource.Version, out var installDate))
                    {
                        resource.InstalledOn = installDate.Date;
                    }
                }
            }

            return resources;
        }
    }
}
