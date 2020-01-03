using System;
using System.Linq;
using Lidarr.Http.Extensions.Pipelines;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using Nancy.Responses.Negotiation;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Instrumentation;
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

        protected override Func<ITypeCatalog, NancyInternalConfiguration> InternalConfiguration
        {
            get
            {
                // We don't support Xml Serialization atm
                return NancyInternalConfiguration.WithOverrides(x =>
                {
                    x.ResponseProcessors.Remove(typeof(ViewProcessor));
                    x.ResponseProcessors.Remove(typeof(XmlProcessor));
                });
            }
        }

        public override void Configure(Nancy.Configuration.INancyEnvironment environment)
        {
            environment.Diagnostics(password: @"password");
        }

        protected override byte[] FavIcon => null;
    }
}
