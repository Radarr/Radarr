using System.Runtime.Serialization;

namespace NzbDrone.Core.NetImport.TMDb
{
    public enum TMDbListType
    {
        [EnumMember(Value = "In Theaters")]
        Theaters = 0,
        [EnumMember(Value = "Popular")]
        Popular = 1,
        [EnumMember(Value = "Top Rated")]
        Top = 2,
        [EnumMember(Value = "Upcoming")]
        Upcoming = 3
    }
}
