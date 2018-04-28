using System.IO;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Analytics;
using NzbDrone.Core.Configuration;

namespace Lidarr.Http.Frontend
{
    public class InitializeJsModule : NancyModule
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IAnalyticsService _analyticsService;


        public InitializeJsModule(IConfigFileProvider configFileProvider,
                                  IAnalyticsService analyticsService)
        {
            _configFileProvider = configFileProvider;
            _analyticsService = analyticsService;

            Get["/initialize.js"] = x => Index();
        }

        private Response Index()
        {
            // TODO: Move away from window.Lidarr and prefetch the information returned here when starting the UI
            return new StreamResponse(GetContentStream, "application/javascript");
        }

        private Stream GetContentStream()
        {
            var urlBase = _configFileProvider.UrlBase;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("window.Lidarr = {");
            writer.WriteLine($"  apiRoot: '{urlBase}/api/v1',");
            writer.WriteLine($"  apiKey: '{_configFileProvider.ApiKey}',");
            writer.WriteLine($"  release: '{BuildInfo.Release}',");
            writer.WriteLine($"  version: '{BuildInfo.Version.ToString()}',");
            writer.WriteLine($"  branch: '{_configFileProvider.Branch.ToLower()}',");
            writer.WriteLine($"  analytics: {_analyticsService.IsEnabled.ToString().ToLowerInvariant()},");
            writer.WriteLine($"  urlBase: '{urlBase}',");
            writer.WriteLine($"  isProduction: {RuntimeInfo.IsProduction.ToString().ToLowerInvariant()}");
            writer.WriteLine("};");

            writer.Flush();
            stream.Position = 0;

            return stream;
        }
    }
}
