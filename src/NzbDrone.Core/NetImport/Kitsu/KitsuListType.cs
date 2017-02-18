using System.Runtime.Serialization;

namespace NzbDrone.Core.NetImport.Kitsu
{
    public enum KitsuListType
    {
        [EnumMember(Value = "Currently Watching")]
        CurrentList = 1,
        [EnumMember(Value = "Plan to Watch")]
        PlannedList = 2,
        [EnumMember(Value = "Completed")]
        CompletedList = 3,
        [EnumMember(Value = "On Hold")]
        OnHoldList = 4,
        [EnumMember(Value = "Dropped")]
        DroppedList = 5
    }
}
