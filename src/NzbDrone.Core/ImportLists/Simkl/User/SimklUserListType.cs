using System.Runtime.Serialization;

namespace NzbDrone.Core.ImportLists.Simkl.User
{
    public enum SimklUserListType
    {
        [EnumMember(Value = "User Watch List")]
        UserWatchList = 0,
        [EnumMember(Value = "User Watched List")]
        UserWatchedList = 1,
        [EnumMember(Value = "User Collection List")]
        UserCollectionList = 2
    }
}
