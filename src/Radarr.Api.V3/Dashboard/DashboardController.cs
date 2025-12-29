using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Analytics;
using Radarr.Http;

namespace Radarr.Api.V3.Dashboard
{
    [V3ApiController]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [Produces("application/json")]
        public DashboardResource GetDashboard()
        {
            return _dashboardService.GetStatistics().ToResource();
        }
    }
}
