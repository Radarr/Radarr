using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Moq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;
using NzbDrone.Core.DecisionEngine.Specifications;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]

    public class ProperSpecificationFixture : CoreTest<ProperSpecification>
    {
        private RemoteAlbum _parseResultMulti;
        private RemoteAlbum _parseResultSingle;
        private TrackFile _firstFile;
        private TrackFile _secondFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _firstFile = new TrackFile { Quality = new QualityModel(Quality.FLAC, new Revision(version: 1)), DateAdded = DateTime.Now };
            _secondFile = new TrackFile { Quality = new QualityModel(Quality.FLAC, new Revision(version: 1)), DateAdded = DateTime.Now };

            var singleAlbumList = new List<Album> { new Album {}, new Album {} };
            var doubleAlbumList = new List<Album> { new Album {}, new Album {}, new Album {} };


            var fakeArtist = Builder<Artist>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.FLAC.Id })
                         .Build();

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

        [Test]
        public void should_return_false_when_trackFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.MP3_256;

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_first_trackFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.MP3_256;
            _secondFile.Quality.Quality = Quality.MP3_256;

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_second_trackFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.MP3_256;
            _secondFile.Quality.Quality = Quality.MP3_256;

            _secondFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_trackFile_was_added_more_than_7_days_ago_but_proper_is_for_better_quality()
        {
            WithFirstFileUpgradable();

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_trackFile_was_added_more_than_7_days_ago_but_is_for_search()
        {
            WithFirstFileUpgradable();

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, new AlbumSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_proper_but_auto_download_propers_is_false()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotUpgrade);

            _firstFile.Quality.Quality = Quality.MP3_256;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_trackFile_was_added_today()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _firstFile.Quality.Quality = Quality.MP3_256;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_propers_are_not_preferred()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _firstFile.Quality.Quality = Quality.MP3_256;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }
    }
}
