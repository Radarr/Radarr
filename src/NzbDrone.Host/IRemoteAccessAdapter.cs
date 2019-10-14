namespace Radarr.Host.AccessControl
{
    public interface IRemoteAccessAdapter
    {
        void MakeAccessible(bool passive);
    }
}
