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
    public class ShouldRefreshArtistFixture : TestBase<ShouldRefreshArtist>
    {
        private Artist _artist;
        
        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateNew()
                                     .With(v => v.Metadata.Value.Status == ArtistStatusType.Continuing)
                                     .Build();

            Mocker.GetMock<IAlbumService>()
                  .Setup(s => s.GetAlbumsByArtist(_artist.Id))
                  .Returns(Builder<Album>.CreateListOfSize(2)
                                           .All()
                                           .With(e => e.ReleaseDate = DateTime.Today.AddDays(-100))
                                           .Build()
                                           .ToList());
        }

        private void GivenArtistIsEnded()
        {
            _artist.Metadata.Value.Status = ArtistStatusType.Ended;
        }

        private void GivenArtistLastRefreshedMonthsAgo()
        {
            _artist.LastInfoSync = DateTime.UtcNow.AddDays(-90);
        }

        private void GivenArtistLastRefreshedYesterday()
        {
            _artist.LastInfoSync = DateTime.UtcNow.AddDays(-1);
        }

        private void GivenArtistLastRefreshedThreeDaysAgo()
        {
            _artist.LastInfoSync = DateTime.UtcNow.AddDays(-3);
        }

        private void GivenArtistLastRefreshedRecently()
        {
            _artist.LastInfoSync = DateTime.UtcNow.AddHours(-7);
        }

        private void GivenRecentlyAired()
        {
            Mocker.GetMock<IAlbumService>()
                              .Setup(s => s.GetAlbumsByArtist(_artist.Id))
                              .Returns(Builder<Album>.CreateListOfSize(2)
                                                       .TheFirst(1)
                                                       .With(e => e.ReleaseDate = DateTime.Today.AddDays(-7))
                                                       .TheLast(1)
                                                       .With(e => e.ReleaseDate = DateTime.Today.AddDays(-100))
                                                       .Build()
                                                       .ToList());
        }

        [Test]
        public void should_return_true_if_running_artist_last_refreshed_more_than_24_hours_ago()
        {
            GivenArtistLastRefreshedThreeDaysAgo();

            Subject.ShouldRefresh(_artist).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_running_artist_last_refreshed_less_than_12_hours_ago()
        {
            GivenArtistLastRefreshedRecently();

            Subject.ShouldRefresh(_artist).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_ended_artist_last_refreshed_yesterday()
        {
            GivenArtistIsEnded();
            GivenArtistLastRefreshedYesterday();

            Subject.ShouldRefresh(_artist).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_artist_last_refreshed_more_than_30_days_ago()
        {
            GivenArtistIsEnded();
            GivenArtistLastRefreshedMonthsAgo();

            Subject.ShouldRefresh(_artist).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_album_released_in_last_30_days()
        {
            GivenArtistIsEnded();
            GivenArtistLastRefreshedYesterday();

            GivenRecentlyAired();

            Subject.ShouldRefresh(_artist).Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_recently_refreshed_ended_show_has_not_aired_for_30_days()
        {
            GivenArtistIsEnded();
            GivenArtistLastRefreshedYesterday();

            Subject.ShouldRefresh(_artist).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_recently_refreshed_ended_show_aired_in_last_30_days()
        {
            GivenArtistIsEnded();
            GivenArtistLastRefreshedRecently();

            GivenRecentlyAired();

            Subject.ShouldRefresh(_artist).Should().BeFalse();
        }
    }
}
