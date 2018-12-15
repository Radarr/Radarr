using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class ImportApprovedTracksFixture : CoreTest<ImportApprovedTracks>
    {
        private List<ImportDecision> _rejectedDecisions;
        private List<ImportDecision> _approvedDecisions;

        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _rejectedDecisions = new List<ImportDecision>();
            _approvedDecisions = new List<ImportDecision>();

            var artist = Builder<Artist>.CreateNew()
                                        .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                        .With(l => l.LanguageProfile = new LanguageProfile
                                        {
                                            Cutoff = Language.Spanish,
                                            Languages = Languages.LanguageFixture.GetDefaultLanguages()
                                        })
                                        .With(s => s.Path = @"C:\Test\Music\Alien Ant Farm".AsOsAgnostic())
                                        .Build();

            var album = Builder<Album>.CreateNew()
                .With(e => e.Artist = artist)
                .Build();

            var release = Builder<AlbumRelease>.CreateNew()
                .With(e => e.AlbumId = album.Id)
                .Build();

            var tracks = Builder<Track>.CreateListOfSize(5)
                                           .Build();



            _rejectedDecisions.Add(new ImportDecision(new LocalTrack(), new Rejection("Rejected!")));
            _rejectedDecisions.Add(new ImportDecision(new LocalTrack(), new Rejection("Rejected!")));
            _rejectedDecisions.Add(new ImportDecision(new LocalTrack(), new Rejection("Rejected!")));

            foreach (var track in tracks)
            {
                _approvedDecisions.Add(new ImportDecision
                                           (
                                           new LocalTrack
                                           {
                                               Artist = artist,
                                               Album = album,
                                               Release = release,
                                               Tracks = new List<Track> { track },
                                               Path = Path.Combine(artist.Path, "Alien Ant Farm - 01 - Pilot.mp3"),
                                               Quality = new QualityModel(Quality.MP3_256),
                                               ParsedTrackInfo = new ParsedTrackInfo
                                               {
                                                   ReleaseGroup = "DRONE"
                                               }
                                           }));
            }

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Setup(s => s.UpgradeTrackFile(It.IsAny<TrackFile>(), It.IsAny<LocalTrack>(), It.IsAny<bool>()))
                  .Returns(new TrackFileMoveResult());

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();

            Mocker.GetMock<IMediaFileService>()
                .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new List<TrackFile>());
        }

        [Test]
        public void should_not_import_any_if_there_are_no_approved_decisions()
        {
            Subject.Import(_rejectedDecisions, false).Where(i => i.Result == ImportResultType.Imported).Should().BeEmpty();

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Add(It.IsAny<TrackFile>()), Times.Never());
        }

        [Test]
        public void should_import_each_approved()
        {
            Subject.Import(_approvedDecisions, false).Should().HaveCount(5);
        }

        [Test]
        public void should_only_import_approved()
        {
            var all = new List<ImportDecision>();
            all.AddRange(_rejectedDecisions);
            all.AddRange(_approvedDecisions);

            var result = Subject.Import(all, false);

            result.Should().HaveCount(all.Count);
            result.Where(i => i.Result == ImportResultType.Imported).Should().HaveCount(_approvedDecisions.Count);
        }

        [Test]
        public void should_only_import_each_track_once()
        {
            var all = new List<ImportDecision>();
            all.AddRange(_approvedDecisions);
            all.Add(new ImportDecision(_approvedDecisions.First().LocalTrack));

            var result = Subject.Import(all, false);

            result.Where(i => i.Result == ImportResultType.Imported).Should().HaveCount(_approvedDecisions.Count);
        }

        [Test]
        public void should_move_new_downloads()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeTrackFile(It.IsAny<TrackFile>(), _approvedDecisions.First().LocalTrack, false),
                          Times.Once());
        }

        [Test]
        public void should_publish_TrackImportedEvent_for_new_downloads()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.IsAny<TrackImportedEvent>()), Times.Once());
        }

        [Test]
        public void should_not_move_existing_files()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, false);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeTrackFile(It.IsAny<TrackFile>(), _approvedDecisions.First().LocalTrack, false),
                          Times.Never());
        }

        [Test]
        public void should_import_larger_files_first()
        {
            var fileDecision = _approvedDecisions.First();
            fileDecision.LocalTrack.Size = 1.Gigabytes();

            var sampleDecision = new ImportDecision
                (new LocalTrack
                {
                    Artist = fileDecision.LocalTrack.Artist,
                    Album = fileDecision.LocalTrack.Album,
                    Tracks = new List<Track> { fileDecision.LocalTrack.Tracks.First() },
                    Path = @"C:\Test\Music\Alien Ant Farm\Alien Ant Farm - 01 - Pilot.mp3".AsOsAgnostic(),
                    Quality = new QualityModel(Quality.MP3_256),
                    Size = 80.Megabytes()
                });


            var all = new List<ImportDecision>();
            all.Add(fileDecision);
            all.Add(sampleDecision);

            var results = Subject.Import(all, false);

            results.Should().HaveCount(all.Count);
            results.Should().ContainSingle(d => d.Result == ImportResultType.Imported);
            results.Should().ContainSingle(d => d.Result == ImportResultType.Imported && d.ImportDecision.LocalTrack.Size == fileDecision.LocalTrack.Size);
        }

        [Test]
        public void should_copy_when_cannot_move_files_downloads()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, new DownloadClientItem { Title = "Alien.Ant.Farm-Truant", CanMoveFiles = false });

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeTrackFile(It.IsAny<TrackFile>(), _approvedDecisions.First().LocalTrack, true), Times.Once());
        }

        [Test]
        public void should_use_override_importmode()
        {
            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, true, new DownloadClientItem { Title = "Alien.Ant.Farm-Truant", CanMoveFiles = false }, ImportMode.Move);

            Mocker.GetMock<IUpgradeMediaFiles>()
                  .Verify(v => v.UpgradeTrackFile(It.IsAny<TrackFile>(), _approvedDecisions.First().LocalTrack, false), Times.Once());
        }

        [Test]
        public void should_delete_existing_metadata_files_with_the_same_path()
        {
            Mocker.GetMock<IMediaFileService>()
                .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Builder<TrackFile>.CreateListOfSize(1).BuildList());

            Subject.Import(new List<ImportDecision> { _approvedDecisions.First() }, false);

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Delete(It.IsAny<TrackFile>(), DeleteMediaFileReason.ManualOverride), Times.Once());
        }

    }
}
