using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.TrackImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Test.MediaFiles.TrackImport.Specifications
{
    [TestFixture]
    public class UpgradeSpecificationFixture : CoreTest<UpgradeSpecification>
    {
        private Artist _artist;
        private Album _album;
        private LocalTrack _localTrack;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateNew()
                                     .With(e => e.QualityProfile = new QualityProfile
                                     {
                                         Items = Qualities.QualityFixture.GetDefaultQualities(),
                                     }).Build();

            _album = Builder<Album>.CreateNew().Build();

            _localTrack = new LocalTrack
            {
                Path = @"C:\Test\Imagine Dragons\Imagine.Dragons.Song.1.mp3",
                Quality = new QualityModel(Quality.MP3_256, new Revision(version: 1)),
                Artist = _artist,
                Album = _album
            };
        }

        [Test]
        public void should_return_true_if_no_existing_trackFile()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 0)
                                                     .With(e => e.TrackFile = null)
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_no_existing_trackFile_for_multi_tracks()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.TrackFileId = 0)
                                                     .With(e => e.TrackFile = null)
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_trackFile()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.MP3_192, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_trackFile_for_multi_tracks()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.MP3_192, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_trackFile()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.FLAC, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_trackFile_for_multi_tracks()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.FLAC, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_one_existing_trackFile_for_multi_track()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .TheFirst(1)
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.MP3_192, new Revision(version: 1))
                                                                                }))
                                                     .TheNext(1)
                                                     .With(e => e.TrackFileId = 2)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.FLAC, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack).Accepted.Should().BeFalse();
        }


        [Test]
        public void should_return_false_if_not_a_revision_upgrade_and_prefers_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                         new TrackFile
                                                         {
                                                             Quality = new QualityModel(Quality.MP3_256, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_not_a_revision_upgrade_and_does_not_prefer_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                         new TrackFile
                                                         {
                                                             Quality = new QualityModel(Quality.MP3_256, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_comparing_to_a_lower_quality_proper()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localTrack.Quality = new QualityModel(Quality.FLAC);

            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                         new TrackFile
                                                         {
                                                             Quality = new QualityModel(Quality.FLAC, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_track_file_is_null()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(null))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack).Accepted.Should().BeTrue();
        }
    }
}
