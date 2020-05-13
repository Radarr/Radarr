using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class MonitoredAlbumSpecificationFixture : CoreTest<MonitoredAlbumSpecification>
    {
        private MonitoredAlbumSpecification _monitoredAlbumSpecification;

        private RemoteBook _parseResultMulti;
        private RemoteBook _parseResultSingle;
        private Author _fakeArtist;
        private Book _firstAlbum;
        private Book _secondAlbum;

        [SetUp]
        public void Setup()
        {
            _monitoredAlbumSpecification = Mocker.Resolve<MonitoredAlbumSpecification>();

            _fakeArtist = Builder<Author>.CreateNew()
                .With(c => c.Monitored = true)
                .Build();

            _firstAlbum = new Book { Monitored = true };
            _secondAlbum = new Book { Monitored = true };

            var singleAlbumList = new List<Book> { _firstAlbum };
            var doubleAlbumList = new List<Book> { _firstAlbum, _secondAlbum };

            _parseResultMulti = new RemoteBook
            {
                Author = _fakeArtist,
                Books = doubleAlbumList
            };

            _parseResultSingle = new RemoteBook
            {
                Author = _fakeArtist,
                Books = singleAlbumList
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
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultSingle, new BookSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_album_is_not_monitored_and_monitoredEpisodesOnly_flag_is_false()
        {
            WithFirstAlbumUnmonitored();
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultSingle, new BookSearchCriteria { MonitoredBooksOnly = false }).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_album_is_not_monitored_and_monitoredEpisodesOnly_flag_is_true()
        {
            WithFirstAlbumUnmonitored();
            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultSingle, new BookSearchCriteria { MonitoredBooksOnly = true }).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_all_albums_are_not_monitored_for_discography_pack_release()
        {
            WithSecondAlbumUnmonitored();
            _parseResultMulti.ParsedBookInfo = new ParsedBookInfo()
            {
                Discography = true
            };

            _monitoredAlbumSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }
    }
}
