using System.Linq;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using Lidarr.Http.Extensions.Pipelines;
using TinyIoC;

namespace Lidarr.Http
{
    public class LidarrBootstrapper : TinyIoCNancyBootstrapper
    {
        private readonly TinyIoCContainer _tinyIoCContainer;
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(LidarrBootstrapper));

        public LidarrBootstrapper(TinyIoCContainer tinyIoCContainer)
        {
            _tinyIoCContainer = tinyIoCContainer;
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            Logger.Info("Starting Web Server");

            if (RuntimeInfo.IsProduction)
            {
                DiagnosticsHook.Disable(pipelines);
            }

            RegisterPipelines(pipelines);

            container.Resolve<DatabaseTarget>().Register();
        }

        private void RegisterPipelines(IPipelines pipelines)
        {
            var pipelineRegistrars = _tinyIoCContainer.ResolveAll<IRegisterNancyPipeline>().OrderBy(v => v.Order).ToList();

            foreach (var registerNancyPipeline in pipelineRegistrars)
            {
                registerNancyPipeline.Register(pipelines);
            }
        }

        protected override TinyIoCContainer GetApplicationContainer()
        {
            return _tinyIoCContainer;
        }

        protected override DiagnosticsConfiguration DiagnosticsConfiguration => new DiagnosticsConfiguration { Password = @"password" };

        protected override byte[] FavIcon => null;
    }
}
