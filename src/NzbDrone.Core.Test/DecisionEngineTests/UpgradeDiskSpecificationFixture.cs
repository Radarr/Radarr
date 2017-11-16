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
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;

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

            _firstFile = new TrackFile { Quality = new QualityModel(Quality.FLAC, new Revision(version: 2)), DateAdded = DateTime.Now, Language = Language.English };
            _secondFile = new TrackFile { Quality = new QualityModel(Quality.FLAC, new Revision(version: 2)), DateAdded = DateTime.Now, Language = Language.English };

            var singleEpisodeList = new List<Album> { new Album {}};
            var doubleEpisodeList = new List<Album> { new Album {}, new Album {}, new Album {} };

            var languages = Languages.LanguageFixture.GetDefaultLanguages(Language.English, Language.Spanish);

            var fakeArtist = Builder<Artist>.CreateNew()
                         .With(c => c.Profile = new Profile { Cutoff = Quality.MP3_512.Id, Items = Qualities.QualityFixture.GetDefaultQualities()})
                         .With(l => l.LanguageProfile = new LanguageProfile { Cutoff = Language.Spanish, Languages = languages })
                         .Build();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesByAlbum(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(new List<TrackFile> { _firstFile, _secondFile });

            _parseResultMulti = new RemoteAlbum
            {
                Artist = fakeArtist,
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_256, new Revision(version: 2)), Language = Language.English },
                Albums = doubleEpisodeList
            };

            _parseResultSingle = new RemoteAlbum
            {
                Artist = fakeArtist,
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_256, new Revision(version: 2)), Language = Language.English },
                Albums = singleEpisodeList
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
                  .Setup(c => c.GetFilesByAlbum(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(new List<TrackFile> { });

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_single_album_doesnt_exist_on_disk()
        {
            _parseResultSingle.Albums = new List<Album>();

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_album_is_upgradable()
        {
            WithFirstFileUpgradable();
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_qualities_are_the_same()
        {
            _firstFile.Quality = new QualityModel(Quality.MP3_512);
            _parseResultSingle.ParsedAlbumInfo.Quality = new QualityModel(Quality.MP3_512);
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_all_tracks_are_not_upgradable()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }
    }
}
