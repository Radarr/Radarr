using System.Runtime.Serialization;

namespace NzbDrone.Core.Notifications.Apprise
{
    public enum AppriseNotificationType
    {
        [EnumMember(Value = "info")]
        Info,

        [EnumMember(Value = "success")]
        Success,

        [EnumMember(Value = "warning")]
        Warning,

        [EnumMember(Value = "failure")]
        Failure,
    }
}
