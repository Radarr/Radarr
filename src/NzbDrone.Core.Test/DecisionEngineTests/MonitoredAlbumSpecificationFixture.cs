using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class MonitoredAlbumSpecificationFixture : CoreTest<MonitoredAlbumSpecification>
    {
        private MonitoredAlbumSpecification _monitoredAlbumSpecification;

        private RemoteAlbum _parseResultMulti;
        private RemoteAlbum _parseResultSingle;
        private Artist _fakeArtist;
        private Album _firstAlbum;
        private Album _secondAlbum;

        [SetUp]
        public void Setup()
        {
            _monitoredAlbumSpecification = Mocker.Resolve<MonitoredAlbumSpecification>();

            _fakeArtist = Builder<Artist>.CreateNew()
                .With(c => c.Monitored = true)
                .Build();

            _firstAlbum = new Album { Monitored = true };
            _secondAlbum = new Album { Monitored = true };


            var singleAlbumList = new List<Album> { _firstAlbum };
            var doubleAlbumList = new List<Album> { _firstAlbum, _secondAlbum };

            _parseResultMulti = new RemoteAlbum
            {
                Artist = _fakeArtist,
                Albums = doubleAlbumList
            };

            _parseResultSingle = new RemoteAlbum
            {
                Artist = _fakeArtist,
                Albums = singleAlbumList
            };
        }

        private void WithFirstAlbumUnmonitored()
        {
            _firstAlbum.Monitored = false;
        }

        private void WithSecondAlbumUnmonitored()
        {
            _secondAlbum.Monitored = false;
        }

        [Test]
        public void setup_should_return_monitored_album_should_return_true()
        {
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void not_monitored_artist_should_be_skipped()
        {
            _fakeArtist.Monitored = false;
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_album_not_monitored_should_return_false()
        {
            WithFirstAlbumUnmonitored();
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void both_albums_not_monitored_should_return_false()
        {
            WithFirstAlbumUnmonitored();
            WithSecondAlbumUnmonitored();
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_first_album_not_monitored_should_return_false()
        {
            WithFirstAlbumUnmonitored();
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_second_album_not_monitored_should_return_false()
        {
            WithSecondAlbumUnmonitored();
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_single_album_search()
        {
            _fakeArtist.Monitored = false;
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultSingle, new AlbumSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_album_is_not_monitored_and_monitoredEpisodesOnly_flag_is_false()
        {
            WithFirstAlbumUnmonitored();
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultSingle, new AlbumSearchCriteria { MonitoredEpisodesOnly = false }).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_album_is_not_monitored_and_monitoredEpisodesOnly_flag_is_true()
        {
            WithFirstAlbumUnmonitored();
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultSingle, new AlbumSearchCriteria{ MonitoredEpisodesOnly = true}).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_all_albums_are_not_monitored_for_discography_pack_release()
        {
            WithSecondAlbumUnmonitored();
            _parseResultMulti.ParsedAlbumInfo = new ParsedAlbumInfo()
            {
                Discography = true
            };

            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }
    }
}
