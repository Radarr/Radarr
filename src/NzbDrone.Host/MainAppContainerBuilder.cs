using System.Collections.Generic;
using Nancy.Bootstrapper;
using Radarr.Http;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.SignalR;

namespace Radarr.Host
{
    public class MainAppContainerBuilder : ContainerBuilderBase
    {
        public static IContainer BuildContainer(StartupContext args)
        {
            var assemblies = new List<string>
                             {
                                 "Radarr.Host",
                                 "Radarr.Core",
                                 "Radarr.Api",
                                 "Radarr.SignalR",
                                 "Radarr.Api.V2",
                                 "Radarr.Http"
                             };

            return new MainAppContainerBuilder(args, assemblies).Container;
        }

        private MainAppContainerBuilder(StartupContext args, List<string> assemblies)
            : base(args, assemblies)
        {
            AutoRegisterImplementations<MessageHub>();

            Container.Register<INancyBootstrapper, RadarrBootstrapper>();

            if (OsInfo.IsWindows)
            {
                Container.Register<INzbDroneServiceFactory, NzbDroneServiceFactory>();
            }
            else
            {
                Container.Register<INzbDroneServiceFactory, DummyNzbDroneServiceFactory>();
            }
        }
    }
}
