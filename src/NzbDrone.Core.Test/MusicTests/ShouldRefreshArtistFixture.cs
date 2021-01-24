using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class ShouldRefreshAuthorFixture : TestBase<ShouldRefreshAuthor>
    {
        private Author _author;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>.CreateNew()
                                     .With(v => v.Metadata.Value.Status == AuthorStatusType.Continuing)
                                     .Build();

            Mocker.GetMock<IBookService>()
                  .Setup(s => s.GetBooksByAuthor(_author.Id))
                  .Returns(Builder<Book>.CreateListOfSize(2)
                                           .All()
                                           .With(e => e.ReleaseDate = DateTime.Today.AddDays(-100))
                                           .Build()
                                           .ToList());
        }

        private void GivenAuthorIsEnded()
        {
            _author.Metadata.Value.Status = AuthorStatusType.Ended;
        }

        private void GivenAuthorLastRefreshedMonthsAgo()
        {
            _author.LastInfoSync = DateTime.UtcNow.AddDays(-90);
        }

        private void GivenAuthorLastRefreshedYesterday()
        {
            _author.LastInfoSync = DateTime.UtcNow.AddDays(-1);
        }

        private void GivenAuthorLastRefreshedThreeDaysAgo()
        {
            _author.LastInfoSync = DateTime.UtcNow.AddDays(-3);
        }

        private void GivenAuthorLastRefreshedRecently()
        {
            _author.LastInfoSync = DateTime.UtcNow.AddHours(-7);
        }

        private void GivenRecentlyAired()
        {
            Mocker.GetMock<IBookService>()
                              .Setup(s => s.GetBooksByAuthor(_author.Id))
                              .Returns(Builder<Book>.CreateListOfSize(2)
                                                       .TheFirst(1)
                                                       .With(e => e.ReleaseDate = DateTime.Today.AddDays(-7))
                                                       .TheLast(1)
                                                       .With(e => e.ReleaseDate = DateTime.Today.AddDays(-100))
                                                       .Build()
                                                       .ToList());
        }

        [Test]
        public void should_return_true_if_running_author_last_refreshed_more_than_24_hours_ago()
        {
            GivenAuthorLastRefreshedThreeDaysAgo();

            Subject.ShouldRefresh(_author).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_running_author_last_refreshed_less_than_12_hours_ago()
        {
            GivenAuthorLastRefreshedRecently();

            Subject.ShouldRefresh(_author).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_ended_author_last_refreshed_yesterday()
        {
            GivenAuthorIsEnded();
            GivenAuthorLastRefreshedYesterday();

            Subject.ShouldRefresh(_author).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_author_last_refreshed_more_than_30_days_ago()
        {
            GivenAuthorIsEnded();
            GivenAuthorLastRefreshedMonthsAgo();

            Subject.ShouldRefresh(_author).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_book_released_in_last_30_days()
        {
            GivenAuthorIsEnded();
            GivenAuthorLastRefreshedYesterday();

            GivenRecentlyAired();

            Subject.ShouldRefresh(_author).Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_recently_refreshed_ended_show_has_not_aired_for_30_days()
        {
            GivenAuthorIsEnded();
            GivenAuthorLastRefreshedYesterday();

            Subject.ShouldRefresh(_author).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_recently_refreshed_ended_show_aired_in_last_30_days()
        {
            GivenAuthorIsEnded();
            GivenAuthorLastRefreshedRecently();

            GivenRecentlyAired();

            Subject.ShouldRefresh(_author).Should().BeFalse();
        }
    }
}
