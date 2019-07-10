using System.Runtime.Serialization;

namespace NzbDrone.Core.NetImport.TMDb
{
    public enum TMDbListType
    {
        [EnumMember(Value = "List")]
        List = 0,
        [EnumMember(Value = "In Theaters")]
        Theaters = 1,
        [EnumMember(Value = "Popular")]
        Popular = 2,
        [EnumMember(Value = "Top Rated")]
        Top = 3,
        [EnumMember(Value = "Upcoming")]
        Upcoming = 4,
        [EnumMember(Value = "People Cast")]
        PeopleCast = 5,
        [EnumMember(Value = "People Crew")]
        PeopleCrew = 6
    }
}
