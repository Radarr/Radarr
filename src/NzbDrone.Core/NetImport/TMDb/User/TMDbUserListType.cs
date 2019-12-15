using System.Runtime.Serialization;

namespace NzbDrone.Core.NetImport.TMDb.User
{
    public enum TMDbUserListType
    {
        [EnumMember(Value = "Watchlist")]
        Watchlist = 1,
        [EnumMember(Value = "Recommendations")]
        Recommendations = 2,
        [EnumMember(Value = "Rated")]
        Rated = 3,
        [EnumMember(Value = "Favorite")]
        Favorite = 4
    }
}
