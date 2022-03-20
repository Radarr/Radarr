using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests
{
    [TestFixture]
    public class ShouldRefreshMovieFixture : TestBase<ShouldRefreshMovie>
    {
        private MovieMetadata _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<MovieMetadata>.CreateNew()
                                     .With(v => v.Status = MovieStatusType.InCinemas)
                                     .With(m => m.PhysicalRelease = DateTime.Today.AddDays(-100))
                                     .Build();
        }

        private void GivenMovieIsAnnouced()
        {
            _movie.Status = MovieStatusType.Announced;
        }

        private void GivenMovieIsReleased()
        {
            _movie.Status = MovieStatusType.Released;
        }

        private void GivenMovieLastRefreshedMonthsAgo()
        {
            _movie.LastInfoSync = DateTime.UtcNow.AddDays(-190);
        }

        private void GivenMovieLastRefreshedYesterday()
        {
            _movie.LastInfoSync = DateTime.UtcNow.AddDays(-1);
        }

        private void GivenMovieLastRefreshedADayAgo()
        {
            _movie.LastInfoSync = DateTime.UtcNow.AddHours(-24);
        }

        private void GivenMovieLastRefreshedRecently()
        {
            _movie.LastInfoSync = DateTime.UtcNow.AddHours(-1);
        }

        private void GivenRecentlyReleased()
        {
            _movie.PhysicalRelease = DateTime.Today.AddDays(-7);
        }

        [Test]
        public void should_return_true_if_in_cinemas_movie_last_refreshed_more_than_12_hours_ago()
        {
            GivenMovieLastRefreshedADayAgo();

            Subject.ShouldRefresh(_movie).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_in_cinemas_movie_last_refreshed_less_than_12_hours_ago()
        {
            GivenMovieLastRefreshedRecently();

            Subject.ShouldRefresh(_movie).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_released_movie_last_refreshed_yesterday()
        {
            GivenMovieIsReleased();
            GivenMovieLastRefreshedYesterday();

            Subject.ShouldRefresh(_movie).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_movie_last_refreshed_more_than_30_days_ago()
        {
            GivenMovieIsReleased();
            GivenMovieLastRefreshedMonthsAgo();

            Subject.ShouldRefresh(_movie).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_episode_aired_in_last_30_days()
        {
            GivenMovieIsReleased();
            GivenMovieLastRefreshedYesterday();

            GivenRecentlyReleased();

            Subject.ShouldRefresh(_movie).Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_recently_refreshed_released_movie_released_30_days()
        {
            GivenMovieIsReleased();
            GivenMovieLastRefreshedYesterday();

            Subject.ShouldRefresh(_movie).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_recently_refreshed_ended_show_aired_in_last_30_days()
        {
            GivenMovieIsReleased();
            GivenMovieLastRefreshedRecently();

            GivenRecentlyReleased();

            Subject.ShouldRefresh(_movie).Should().BeFalse();
        }
    }
}
