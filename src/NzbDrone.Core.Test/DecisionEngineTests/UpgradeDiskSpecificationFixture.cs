using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Moq;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class UpgradeDiskSpecificationFixture : CoreTest<UpgradeDiskSpecification>
    {
        private RemoteAlbum _parseResultMulti;
        private RemoteAlbum _parseResultSingle;
        private TrackFile _firstFile;
        private TrackFile _secondFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _firstFile = new TrackFile { Quality = new QualityModel(Quality.FLAC, new Revision(version: 2)), DateAdded = DateTime.Now };
            _secondFile = new TrackFile { Quality = new QualityModel(Quality.FLAC, new Revision(version: 2)), DateAdded = DateTime.Now };

            var singleAlbumList = new List<Album> { new Album {}};
            var doubleAlbumList = new List<Album> { new Album {}, new Album {}, new Album {} };

            var fakeArtist = Builder<Artist>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile
                         {
                             UpgradeAllowed = true,
                             Cutoff = Quality.MP3_320.Id,
                             Items = Qualities.QualityFixture.GetDefaultQualities()
                         })
                         .Build();

            Mocker.GetMock<ITrackService>()
                .Setup(c => c.TracksWithoutFiles(It.IsAny<int>()))
                .Returns(new List<Track>());

            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesByAlbum(It.IsAny<int>()))
                  .Returns(new List<TrackFile> { _firstFile, _secondFile });

            _parseResultMulti = new RemoteAlbum
            {
                Artist = fakeArtist,
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_256, new Revision(version: 2)) },
                Albums = doubleAlbumList
            };

            _parseResultSingle = new RemoteAlbum
            {
                Artist = fakeArtist,
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_256, new Revision(version: 2)) },
                Albums = singleAlbumList
            };

        }

        private void WithFirstFileUpgradable()
        {
            _firstFile.Quality = new QualityModel(Quality.MP3_192);
        }

        private void WithSecondFileUpgradable()
        {
            _secondFile.Quality = new QualityModel(Quality.MP3_192);
        }

        [Test]
        public void should_return_true_if_album_has_no_existing_file()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesByAlbum(It.IsAny<int>()))
                  .Returns(new List<TrackFile> { });

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_track_is_missing()
        {
            Mocker.GetMock<ITrackService>()
                  .Setup(c => c.TracksWithoutFiles(It.IsAny<int>()))
                .Returns(new List<Track> { new Track() });

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_only_query_db_for_missing_tracks_once()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();

            Mocker.GetMock<ITrackService>()
                .Verify(c => c.TracksWithoutFiles(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_return_true_if_single_album_doesnt_exist_on_disk()
        {
            _parseResultSingle.Albums = new List<Album>();

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
