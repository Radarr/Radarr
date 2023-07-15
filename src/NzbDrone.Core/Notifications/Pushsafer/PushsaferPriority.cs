namespace NzbDrone.Core.Notifications.Pushsafer
{
    public enum PushsaferPriority
    {
        Silent = -2,
        Quiet = -1,
        Normal = 0,
        High = 1,
        Emergency = 2
    }
}
