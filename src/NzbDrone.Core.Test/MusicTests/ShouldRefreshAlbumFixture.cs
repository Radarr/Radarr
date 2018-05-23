using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class ShouldRefreshAlbumFixture : TestBase<ShouldRefreshAlbum>
    {
        private Album _album;
        
        [SetUp]
        public void Setup()
        {
            _album = Builder<Album>.CreateNew()
                                   .With(e=>e.ReleaseDate = DateTime.Today.AddDays(-100))
                                   .Build();
        }

        private void GivenAlbumLastRefreshedMonthsAgo()
        {
            _album.LastInfoSync = DateTime.UtcNow.AddDays(-90);
        }

        private void GivenAlbumLastRefreshedYesterday()
        {
            _album.LastInfoSync = DateTime.UtcNow.AddDays(-1);
        }

        private void GivenAlbumLastRefreshedRecently()
        {
            _album.LastInfoSync = DateTime.UtcNow.AddHours(-7);
        }

        private void GivenRecentlyReleased()
        {
            _album.ReleaseDate = DateTime.Today.AddDays(-7);
        }

        private void GivenFutureRelease()
        {
            _album.ReleaseDate = DateTime.Today.AddDays(7);
        }

        [Test]
        public void should_return_false_if_album_last_refreshed_less_than_12_hours_ago()
        {
            GivenAlbumLastRefreshedRecently();

            Subject.ShouldRefresh(_album).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_album_last_refreshed_more_than_30_days_ago()
        {
            GivenAlbumLastRefreshedMonthsAgo();

            Subject.ShouldRefresh(_album).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_album_released_in_last_30_days()
        {
            GivenAlbumLastRefreshedYesterday();

            GivenRecentlyReleased();

            Subject.ShouldRefresh(_album).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_album_releases_in_future()
        {
            GivenAlbumLastRefreshedYesterday();

            GivenFutureRelease();

            Subject.ShouldRefresh(_album).Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_recently_refreshed_album_released_over_30_days_ago()
        {
            GivenAlbumLastRefreshedYesterday();

            Subject.ShouldRefresh(_album).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_recently_refreshed_album_released_in_last_30_days()
        {
            GivenAlbumLastRefreshedRecently();

            GivenRecentlyReleased();

            Subject.ShouldRefresh(_album).Should().BeFalse();
        }
    }
}
