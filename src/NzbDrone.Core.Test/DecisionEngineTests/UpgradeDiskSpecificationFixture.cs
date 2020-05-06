using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    [Ignore("Pending Readarr fixes")]
    public class UpgradeDiskSpecificationFixture : CoreTest<UpgradeDiskSpecification>
    {
        private RemoteAlbum _parseResultMulti;
        private RemoteAlbum _parseResultSingle;
        private BookFile _firstFile;
        private BookFile _secondFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _firstFile = new BookFile { Quality = new QualityModel(Quality.FLAC, new Revision(version: 2)), DateAdded = DateTime.Now };
            _secondFile = new BookFile { Quality = new QualityModel(Quality.FLAC, new Revision(version: 2)), DateAdded = DateTime.Now };

            var singleAlbumList = new List<Book> { new Book { BookFiles = new List<BookFile>() } };
            var doubleAlbumList = new List<Book> { new Book { BookFiles = new List<BookFile>() }, new Book { BookFiles = new List<BookFile>() }, new Book { BookFiles = new List<BookFile>() } };

            var fakeArtist = Builder<Author>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile
                         {
                             UpgradeAllowed = true,
                             Cutoff = Quality.MP3_320.Id,
                             Items = Qualities.QualityFixture.GetDefaultQualities()
                         })
                         .Build();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesByAlbum(It.IsAny<int>()))
                  .Returns(new List<BookFile> { _firstFile, _secondFile });

            _parseResultMulti = new RemoteAlbum
            {
                Artist = fakeArtist,
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
                Albums = doubleAlbumList
            };

            _parseResultSingle = new RemoteAlbum
            {
                Artist = fakeArtist,
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
                Albums = singleAlbumList
            };
        }

        private void WithFirstFileUpgradable()
        {
            _firstFile.Quality = new QualityModel(Quality.MP3_320);
        }

        private void WithSecondFileUpgradable()
        {
            _secondFile.Quality = new QualityModel(Quality.MP3_320);
        }

        [Test]
        public void should_return_true_if_album_has_no_existing_file()
        {
            _parseResultSingle.Albums.First().BookFiles = new List<BookFile>();

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_track_is_missing()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_only_query_db_for_missing_tracks_once()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_single_album_doesnt_exist_on_disk()
        {
            _parseResultSingle.Albums = new List<Book>();

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_all_files_are_upgradable()
        {
            WithFirstFileUpgradable();
            WithSecondFileUpgradable();
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_qualities_are_the_same()
        {
            _firstFile.Quality = new QualityModel(Quality.MP3_320);
            _secondFile.Quality = new QualityModel(Quality.MP3_320);
            _parseResultSingle.ParsedAlbumInfo.Quality = new QualityModel(Quality.MP3_320);
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_all_tracks_are_not_upgradable()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_if_some_tracks_are_upgradable_and_none_are_downgrades()
        {
            WithFirstFileUpgradable();
            _parseResultSingle.ParsedAlbumInfo.Quality = _secondFile.Quality;
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_if_some_tracks_are_upgradable_and_some_are_downgrades()
        {
            WithFirstFileUpgradable();
            _parseResultSingle.ParsedAlbumInfo.Quality = new QualityModel(Quality.MP3_320);
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }
    }
}
