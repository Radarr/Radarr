using System.Runtime.Serialization;

namespace NzbDrone.Core.NetImport.Trakt
{
    public enum TraktListType
    {
        [EnumMember(Value = "User Watch List")]
        UserWatchList = 0,
        [EnumMember(Value = "User Watched List")]
        UserWatchedList = 1,
        [EnumMember(Value = "User Custom List")]
        UserCustomList = 2,

        [EnumMember(Value = "Trending Movies")]
        TrendingMovies = 3,
        [EnumMember(Value = "Popular Movies")]
        PopularMovies = 4,
        [EnumMember(Value = "Top Anticipated Movies")]
        AnticipatedMovies = 5,
        [EnumMember(Value = "Top Box Office Movies")]
        BoxOfficeMovies = 6,

        [EnumMember(Value = "Top Watched Movies By Week")]
        TopWatchedByWeek = 7,
        [EnumMember(Value = "Top Watched Movies By Month")]
        TopWatchedByMonth = 8,
        [EnumMember(Value = "Top Watched Movies By Year")]
        TopWatchedByYear = 9,
        [EnumMember(Value = "Top Watched Movies Of All Time")]
        TopWatchedByAllTime = 10
    }
}
