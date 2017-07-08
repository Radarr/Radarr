using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Music;

namespace NzbDrone.Api.AlbumPass
{
    public class AlbumStudioModule : NzbDroneApiModule
    {
        private readonly IAlbumMonitoredService _albumMonitoredService;

        public AlbumStudioModule(IAlbumMonitoredService albumMonitoredService)
            : base("/albumstudio")
        {
            _albumMonitoredService = albumMonitoredService;
            Post["/"] = artist => UpdateAll();
        }

        private Response UpdateAll()
        {
            //Read from request
            var request = Request.Body.FromJson<AlbumStudioResource>();

            foreach (var s in request.Artist)
            {
                _albumMonitoredService.SetAlbumMonitoredStatus(s, request.MonitoringOptions);
            }

            return "ok".AsResponse(HttpStatusCode.Accepted);
        }
    }
}
