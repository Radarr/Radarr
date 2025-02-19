using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Trakt.Popular
{
    public enum TraktPopularListType
    {
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTrendingMovies")]
        Trending = 0,
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypePopularMovies")]
        Popular = 1,
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTopAnticipatedMovies")]
        Anticipated = 2,
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTopBoxOfficeMovies")]
        BoxOffice = 3,

        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTopWatchedMoviesByWeek")]
        TopWatchedByWeek = 4,
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTopWatchedMoviesByMonth")]
        TopWatchedByMonth = 5,
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTopWatchedMoviesByYear")]
        TopWatchedByYear = 6,
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTopWatchedMoviesOfAllTime")]
        TopWatchedByAllTime = 7,
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeRecommendedMoviesByWeek")]
        RecommendedByWeek = 8,
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeRecommendedMoviesByMonth")]
        RecommendedByMonth = 9,
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeRecommendedMoviesByYear")]
        RecommendedByYear = 10,
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeRecommendedMoviesOfAllTime")]
        RecommendedByAllTime = 11
    }
}
