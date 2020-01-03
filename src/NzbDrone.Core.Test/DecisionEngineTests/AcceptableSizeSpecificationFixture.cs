using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class AcceptableSizeSpecificationFixture : CoreTest<AcceptableSizeSpecification>
    {
        private const int HIGH_KBPS_BITRATE = 1600;
        private const int TWENTY_MINUTE_EP_MILLIS = 20 * 60 * 1000;
        private const int FORTY_FIVE_MINUTE_LP_MILLIS = 45 * 60 * 1000;
        private RemoteAlbum _parseResultMultiSet;
        private RemoteAlbum _parseResultMulti;
        private RemoteAlbum _parseResultSingle;
        private Artist _artist;
        private QualityDefinition _qualityType;

        private Album AlbumBuilder(int id = 0)
        {
            return new Album
            {
                Id = id,
                AlbumReleases = new List<AlbumRelease>
                {
                    new AlbumRelease
                                               {
                                                   Duration = 0,
                                                   Monitored = true
                                               }
                }
            };
        }

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateNew()
                .Build();

            _parseResultMultiSet = new RemoteAlbum
            {
                Artist = _artist,
                Release = new ReleaseInfo(),
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_192, new Revision(version: 2)) },
                Albums = new List<Album> { AlbumBuilder(), AlbumBuilder(), AlbumBuilder(), AlbumBuilder(), AlbumBuilder(), AlbumBuilder() }
            };

            _parseResultMulti = new RemoteAlbum
            {
                Artist = _artist,
                Release = new ReleaseInfo(),
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_192, new Revision(version: 2)) },
                Albums = new List<Album> { AlbumBuilder(), AlbumBuilder() }
            };

            _parseResultSingle = new RemoteAlbum
            {
                Artist = _artist,
                Release = new ReleaseInfo(),
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_192, new Revision(version: 2)) },
                Albums = new List<Album> { AlbumBuilder(2) }
            };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            _qualityType = Builder<QualityDefinition>.CreateNew()
                .With(q => q.MinSize = 150)
                .With(q => q.MaxSize = 210)
                .With(q => q.Quality = Quality.MP3_192)
                .Build();

            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.Get(Quality.MP3_192)).Returns(_qualityType);

            Mocker.GetMock<IAlbumService>().Setup(
                s => s.GetAlbumsByArtist(It.IsAny<int>()))
                .Returns(new List<Album>()
                {
                    AlbumBuilder(), AlbumBuilder(), AlbumBuilder(), AlbumBuilder(), AlbumBuilder(),
                    AlbumBuilder(), AlbumBuilder(), AlbumBuilder(), AlbumBuilder(2), AlbumBuilder()
                });
        }

        private void GivenLastAlbum()
        {
            Mocker.GetMock<IAlbumService>().Setup(
                s => s.GetAlbumsByArtist(It.IsAny<int>()))
                .Returns(new List<Album>
                {
                    AlbumBuilder(), AlbumBuilder(), AlbumBuilder(), AlbumBuilder(), AlbumBuilder(),
                    AlbumBuilder(), AlbumBuilder(), AlbumBuilder(), AlbumBuilder(), AlbumBuilder(2)
                });
        }

        [TestCase(TWENTY_MINUTE_EP_MILLIS, 20, false)]
        [TestCase(TWENTY_MINUTE_EP_MILLIS, 25, true)]
        [TestCase(TWENTY_MINUTE_EP_MILLIS, 35, false)]
        [TestCase(FORTY_FIVE_MINUTE_LP_MILLIS, 45, false)]
        [TestCase(FORTY_FIVE_MINUTE_LP_MILLIS, 55, true)]
        [TestCase(FORTY_FIVE_MINUTE_LP_MILLIS, 75, false)]
        public void single_album(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _parseResultSingle.Albums.Select(c =>
            {
                c.AlbumReleases.Value[0].Duration = runtime;
                return c;
            }).ToList();
            _parseResultSingle.Artist = _artist;
            _parseResultSingle.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().Be(expectedResult);
        }

        [TestCase(TWENTY_MINUTE_EP_MILLIS, 20 * 2, false)]
        [TestCase(TWENTY_MINUTE_EP_MILLIS, 25 * 2, true)]
        [TestCase(TWENTY_MINUTE_EP_MILLIS, 35 * 2, false)]
        [TestCase(FORTY_FIVE_MINUTE_LP_MILLIS, 45 * 2, false)]
        [TestCase(FORTY_FIVE_MINUTE_LP_MILLIS, 55 * 2, true)]
        [TestCase(FORTY_FIVE_MINUTE_LP_MILLIS, 75 * 2, false)]
        public void multi_album(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _parseResultMulti.Albums.Select(c =>
            {
                c.AlbumReleases.Value[0].Duration = runtime;
                return c;
            }).ToList();
            _parseResultMulti.Artist = _artist;
            _parseResultMulti.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().Be(expectedResult);
        }

        [TestCase(TWENTY_MINUTE_EP_MILLIS, 20 * 6, false)]
        [TestCase(TWENTY_MINUTE_EP_MILLIS, 25 * 6, true)]
        [TestCase(TWENTY_MINUTE_EP_MILLIS, 35 * 6, false)]
        [TestCase(FORTY_FIVE_MINUTE_LP_MILLIS, 45 * 6, false)]
        [TestCase(FORTY_FIVE_MINUTE_LP_MILLIS, 55 * 6, true)]
        [TestCase(FORTY_FIVE_MINUTE_LP_MILLIS, 75 * 6, false)]
        public void multiset_album(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _parseResultMultiSet.Albums.Select(c =>
            {
                c.AlbumReleases.Value[0].Duration = runtime;
                return c;
            }).ToList();
            _parseResultMultiSet.Artist = _artist;
            _parseResultMultiSet.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(_parseResultMultiSet, null).Accepted.Should().Be(expectedResult);
        }

        [Test]
        public void should_return_true_if_size_is_zero()
        {
            GivenLastAlbum();
            _parseResultSingle.Albums.Select(c =>
            {
                c.AlbumReleases.Value[0].Duration = TWENTY_MINUTE_EP_MILLIS;
                return c;
            }).ToList();
            _parseResultSingle.Artist = _artist;
            _parseResultSingle.Release.Size = 0;
            _qualityType.MinSize = 150;
            _qualityType.MaxSize = 210;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_20_minute()
        {
            GivenLastAlbum();
            _parseResultSingle.Albums.Select(c =>
            {
                c.AlbumReleases.Value[0].Duration = TWENTY_MINUTE_EP_MILLIS;
                return c;
            }).ToList();
            _parseResultSingle.Artist = _artist;
            _parseResultSingle.Release.Size = (HIGH_KBPS_BITRATE * 128) * (TWENTY_MINUTE_EP_MILLIS / 1000);
            _qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_45_minute()
        {
            GivenLastAlbum();
            _parseResultSingle.Albums.Select(c =>
            {
                c.AlbumReleases.Value[0].Duration = FORTY_FIVE_MINUTE_LP_MILLIS;
                return c;
            }).ToList();
            _parseResultSingle.Artist = _artist;
            _parseResultSingle.Release.Size = (HIGH_KBPS_BITRATE * 128) * (FORTY_FIVE_MINUTE_LP_MILLIS / 1000);
            _qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }
    }
}
