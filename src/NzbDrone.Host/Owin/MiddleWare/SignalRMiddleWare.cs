using System;
using Microsoft.AspNet.SignalR;
using NzbDrone.Common.Composition;
using NzbDrone.SignalR;
using Owin;

namespace Radarr.Host.Owin.MiddleWare
{
    public class SignalRMiddleWare : IOwinMiddleWare
    {
        public int Order => 1;

        public SignalRMiddleWare(IContainer container)
        {
            SignalRDependencyResolver.Register(container);
            SignalRJsonSerializer.Register();

            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromMinutes(3);
        }

        public void Attach(IAppBuilder appBuilder)
        {
            appBuilder.MapConnection("/signalr", typeof(NzbDronePersistentConnection), new ConnectionConfiguration());
        }
    }
}
