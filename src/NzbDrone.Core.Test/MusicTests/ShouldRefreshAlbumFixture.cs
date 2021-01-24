using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class ShouldRefreshBookFixture : TestBase<ShouldRefreshBook>
    {
        private Book _book;

        [SetUp]
        public void Setup()
        {
            _book = Builder<Book>.CreateNew()
                                   .With(e => e.ReleaseDate = DateTime.Today.AddDays(-100))
                                   .Build();
        }

        private void GivenBookLastRefreshedMonthsAgo()
        {
            _book.LastInfoSync = DateTime.UtcNow.AddDays(-90);
        }

        private void GivenBookLastRefreshedYesterday()
        {
            _book.LastInfoSync = DateTime.UtcNow.AddDays(-1);
        }

        private void GivenBookLastRefreshedRecently()
        {
            _book.LastInfoSync = DateTime.UtcNow.AddHours(-7);
        }

        private void GivenRecentlyReleased()
        {
            _book.ReleaseDate = DateTime.Today.AddDays(-7);
        }

        private void GivenFutureRelease()
        {
            _book.ReleaseDate = DateTime.Today.AddDays(7);
        }

        [Test]
        public void should_return_false_if_book_last_refreshed_less_than_12_hours_ago()
        {
            GivenBookLastRefreshedRecently();

            Subject.ShouldRefresh(_book).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_book_last_refreshed_more_than_30_days_ago()
        {
            GivenBookLastRefreshedMonthsAgo();

            Subject.ShouldRefresh(_book).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_book_released_in_last_30_days()
        {
            GivenBookLastRefreshedYesterday();

            GivenRecentlyReleased();

            Subject.ShouldRefresh(_book).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_book_releases_in_future()
        {
            GivenBookLastRefreshedYesterday();

            GivenFutureRelease();

            Subject.ShouldRefresh(_book).Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_recently_refreshed_book_released_over_30_days_ago()
        {
            GivenBookLastRefreshedYesterday();

            Subject.ShouldRefresh(_book).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_recently_refreshed_book_released_in_last_30_days()
        {
            GivenBookLastRefreshedRecently();

            GivenRecentlyReleased();

            Subject.ShouldRefresh(_book).Should().BeFalse();
        }
    }
}
