using System.Runtime.Serialization;

namespace NzbDrone.Core.ImportLists.TMDb.Popular
{
    public enum TMDbPopularListType
    {
        [EnumMember(Value = "In Theaters")]
        Theaters = 1,
        [EnumMember(Value = "Popular")]
        Popular = 2,
        [EnumMember(Value = "Top Rated")]
        Top = 3,
        [EnumMember(Value = "Upcoming")]
        Upcoming = 4
    }
}
