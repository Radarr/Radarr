using System.Runtime.Serialization;

namespace NzbDrone.Core.NetImport.Trakt.Popular
{
    public enum TraktPopularListType
    {
        [EnumMember(Value = "Trending Movies")]
        Trending = 0,
        [EnumMember(Value = "Popular Movies")]
        Popular = 1,
        [EnumMember(Value = "Top Anticipated Movies")]
        Anticipated = 2,
        [EnumMember(Value = "Top Box Office Movies")]
        BoxOffice = 3,

        [EnumMember(Value = "Top Watched Movies By Week")]
        TopWatchedByWeek = 4,
        [EnumMember(Value = "Top Watched Movies By Month")]
        TopWatchedByMonth = 5,
        [EnumMember(Value = "Top Watched Movies By Year")]
        TopWatchedByYear = 6,
        [EnumMember(Value = "Top Watched Movies Of All Time")]
        TopWatchedByAllTime = 7
    }
}
