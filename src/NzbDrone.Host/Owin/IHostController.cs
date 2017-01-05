namespace Radarr.Host.Owin
{
    public interface IHostController
    {
        void StartServer();
        void StopServer();
    }
}