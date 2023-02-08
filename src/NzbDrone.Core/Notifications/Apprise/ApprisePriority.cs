using System.Runtime.Serialization;

namespace NzbDrone.Core.Notifications.Apprise
{
    public enum ApprisePriority
    {
        [EnumMember(Value = "info")]
        Info = 0,

        [EnumMember(Value = "success")]
        Success,

        [EnumMember(Value = "warning")]
        Warning,

        [EnumMember(Value = "failure")]
        Failure,
    }
}
