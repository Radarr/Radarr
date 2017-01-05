using Owin;

namespace Radarr.Host.Owin.MiddleWare
{
    public interface IOwinMiddleWare
    {
        int Order { get; }
        void Attach(IAppBuilder appBuilder);
    }
}