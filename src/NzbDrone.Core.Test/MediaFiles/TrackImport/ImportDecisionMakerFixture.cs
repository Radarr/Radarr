using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.IO.Abstractions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;
using FizzWare.NBuilder;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.TrackImport.Aggregation;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using System.IO.Abstractions.TestingHelpers;

namespace NzbDrone.Core.Test.MediaFiles.TrackImport
{
    [TestFixture]
    public class ImportDecisionMakerFixture : FileSystemTest<ImportDecisionMaker>
    {
        private List<IFileInfo> _fileInfos;
        private LocalTrack _localTrack;
        private Artist _artist;
        private AlbumRelease _albumRelease;
        private QualityModel _quality;

        private Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>> _albumpass1;
        private Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>> _albumpass2;
        private Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>> _albumpass3;

        private Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>> _albumfail1;
        private Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>> _albumfail2;
        private Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>> _albumfail3;

        
        private Mock<IImportDecisionEngineSpecification<LocalTrack>> _pass1;
        private Mock<IImportDecisionEngineSpecification<LocalTrack>> _pass2;
        private Mock<IImportDecisionEngineSpecification<LocalTrack>> _pass3;

        private Mock<IImportDecisionEngineSpecification<LocalTrack>> _fail1;
        private Mock<IImportDecisionEngineSpecification<LocalTrack>> _fail2;
        private Mock<IImportDecisionEngineSpecification<LocalTrack>> _fail3;

        [SetUp]
        public void Setup()
        {
            _albumpass1 = new Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>>();
            _albumpass2 = new Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>>();
            _albumpass3 = new Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>>();

            _albumfail1 = new Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>>();
            _albumfail2 = new Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>>();
            _albumfail3 = new Mock<IImportDecisionEngineSpecification<LocalAlbumRelease>>();

            
            _pass1 = new Mock<IImportDecisionEngineSpecification<LocalTrack>>();
            _pass2 = new Mock<IImportDecisionEngineSpecification<LocalTrack>>();
            _pass3 = new Mock<IImportDecisionEngineSpecification<LocalTrack>>();

            _fail1 = new Mock<IImportDecisionEngineSpecification<LocalTrack>>();
            _fail2 = new Mock<IImportDecisionEngineSpecification<LocalTrack>>();
            _fail3 = new Mock<IImportDecisionEngineSpecification<LocalTrack>>();

            _albumpass1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>())).Returns(Decision.Accept());
            _albumpass2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>())).Returns(Decision.Accept());
            _albumpass3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>())).Returns(Decision.Accept());

            _albumfail1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>())).Returns(Decision.Reject("_albumfail1"));
            _albumfail2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>())).Returns(Decision.Reject("_albumfail2"));
            _albumfail3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>())).Returns(Decision.Reject("_albumfail3"));
            
            _pass1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Accept());
            _pass2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Accept());
            _pass3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Accept());

            _fail1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Reject("_fail1"));
            _fail2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Reject("_fail2"));
            _fail3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Reject("_fail3"));

            _artist = Builder<Artist>.CreateNew()
                                     .With(e => e.QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                     .Build();

            _albumRelease = Builder<AlbumRelease>.CreateNew()
                .Build();

            _quality = new QualityModel(Quality.MP3_256);

            _localTrack = new LocalTrack
            {
                Artist = _artist,
                Quality = _quality,
                Tracks = new List<Track> { new Track() },
                Path = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV.avi".AsOsAgnostic()
            };

            GivenAudioFiles(new List<string> { @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV.avi".AsOsAgnostic() });

            Mocker.GetMock<IIdentificationService>()
                .Setup(s => s.Identify(It.IsAny<List<LocalTrack>>(), It.IsAny<Artist>(), It.IsAny<Album>(), It.IsAny<AlbumRelease>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns((List<LocalTrack> tracks, Artist artist, Album album, AlbumRelease release, bool newDownload, bool singleRelease, bool includeExisting) => {
                        var ret = new LocalAlbumRelease(tracks);
                        ret.AlbumRelease = _albumRelease;
                        return new List<LocalAlbumRelease> { ret };
                    });

            Mocker.GetMock<IMediaFileService>()
                .Setup(c => c.FilterUnchangedFiles(It.IsAny<List<IFileInfo>>(), It.IsAny<Artist>(), It.IsAny<FilterFilesType>()))
                .Returns((List<IFileInfo> files, Artist artist, FilterFilesType filter) => files);

            GivenSpecifications(_albumpass1);
        }

        private void GivenSpecifications<T>(params Mock<IImportDecisionEngineSpecification<T>>[] mocks)
        {
            Mocker.SetConstant(mocks.Select(c => c.Object));
        }

        private void GivenAudioFiles(IEnumerable<string> videoFiles)
        {
            foreach (var file in videoFiles)
            {
                FileSystem.AddFile(file, new MockFileData(string.Empty));
            }

            _fileInfos = videoFiles.Select(x => DiskProvider.GetFileInfo(x)).ToList();
        }

        private void GivenAugmentationSuccess()
        {
            Mocker.GetMock<IAugmentingService>()
                  .Setup(s => s.Augment(It.IsAny<LocalTrack>(), It.IsAny<bool>()))
                  .Callback<LocalTrack, bool>((localTrack, otherFiles) =>
                  {
                      localTrack.Tracks = _localTrack.Tracks;
                  });
        }

        [Test]
        public void should_call_all_album_specifications()
        {
            var downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();
            GivenAugmentationSuccess();
            GivenSpecifications(_albumpass1, _albumpass2, _albumpass3, _albumfail1, _albumfail2, _albumfail3);

            Subject.GetImportDecisions(_fileInfos, new Artist(), null, null, downloadClientItem, null, FilterFilesType.None, false, false, false);

            _albumfail1.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>()), Times.Once());
            _albumfail2.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>()), Times.Once());
            _albumfail3.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>()), Times.Once());
            _albumpass1.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>()), Times.Once());
            _albumpass2.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>()), Times.Once());
            _albumpass3.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalAlbumRelease>()), Times.Once());
        }

        [Test]
        public void should_call_all_track_specifications_if_album_accepted()
        {
            var downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();
            GivenAugmentationSuccess();
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            Subject.GetImportDecisions(_fileInfos, new Artist(), null, null, downloadClientItem, null, FilterFilesType.None, false, false, false);

            _fail1.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Once());
            _fail2.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Once());
            _fail3.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Once());
            _pass1.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Once());
            _pass2.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Once());
            _pass3.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Once());
        }

        [Test]
        public void should_call_no_track_specifications_if_album_rejected()
        {
            var downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();
            GivenAugmentationSuccess();
            GivenSpecifications(_albumpass1, _albumpass2, _albumpass3, _albumfail1, _albumfail2, _albumfail3);
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            Subject.GetImportDecisions(_fileInfos, new Artist(), null, null, downloadClientItem, null, FilterFilesType.None, false, false, false);

            _fail1.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Never());
            _fail2.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Never());
            _fail3.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Never());
            _pass1.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Never());
            _pass2.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Never());
            _pass3.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>()), Times.Never());
        }

        [Test]
        public void should_return_rejected_if_only_album_spec_fails()
        {
            GivenSpecifications(_albumfail1);
            GivenSpecifications(_pass1);

            var result = Subject.GetImportDecisions(_fileInfos, new Artist(), FilterFilesType.None, false);

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_rejected_if_only_track_spec_fails()
        {
            GivenSpecifications(_albumpass1);
            GivenSpecifications(_fail1);

            var result = Subject.GetImportDecisions(_fileInfos, new Artist(), FilterFilesType.None, false);

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_rejected_if_one_album_spec_fails()
        {
            GivenSpecifications(_albumpass1, _albumfail1, _albumpass2, _albumpass3);
            GivenSpecifications(_pass1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_fileInfos, new Artist(), FilterFilesType.None, false);

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_rejected_if_one_track_spec_fails()
        {
            GivenSpecifications(_albumpass1, _albumpass2, _albumpass3);
            GivenSpecifications(_pass1, _fail1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_fileInfos, new Artist(), FilterFilesType.None, false);

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_approved_if_all_specs_pass()
        {
            GivenAugmentationSuccess();
            GivenSpecifications(_albumpass1, _albumpass2, _albumpass3);
            GivenSpecifications(_pass1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_fileInfos, new Artist(), FilterFilesType.None, false);

            result.Single().Approved.Should().BeTrue();
        }

        [Test]
        public void should_have_same_number_of_rejections_as_specs_that_failed()
        {
            GivenAugmentationSuccess();
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            var result = Subject.GetImportDecisions(_fileInfos, new Artist(), FilterFilesType.None, false);
            result.Single().Rejections.Should().HaveCount(3);
        }

        [Test]
        public void should_not_blowup_the_process_due_to_failed_augment()
        {
            GivenSpecifications(_pass1);

            Mocker.GetMock<IAugmentingService>()
                  .Setup(c => c.Augment(It.IsAny<LocalTrack>(), It.IsAny<bool>()))
                  .Throws<TestException>();

            GivenAudioFiles(new []
                {
                    @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV".AsOsAgnostic(),
                    @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV".AsOsAgnostic(),
                    @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV".AsOsAgnostic()
                });

            Subject.GetImportDecisions(_fileInfos, _artist, FilterFilesType.None, false);

            Mocker.GetMock<IAugmentingService>()
                  .Verify(c => c.Augment(It.IsAny<LocalTrack>(), It.IsAny<bool>()), Times.Exactly(_fileInfos.Count));

            ExceptionVerification.ExpectedErrors(3);
        }

        [Test]
        public void should_not_throw_if_release_not_identified()
        {
            GivenSpecifications(_pass1);

            GivenAudioFiles(new []
                {
                    @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV".AsOsAgnostic(),
                    @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV".AsOsAgnostic(),
                    @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV".AsOsAgnostic()
                });

            Mocker.GetMock<IIdentificationService>()
                .Setup(s => s.Identify(It.IsAny<List<LocalTrack>>(), It.IsAny<Artist>(), It.IsAny<Album>(), It.IsAny<AlbumRelease>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns((List<LocalTrack> tracks, Artist artist, Album album, AlbumRelease release, bool newDownload, bool singleRelease, bool includeExisting) => {
                        return new List<LocalAlbumRelease> { new LocalAlbumRelease(tracks) };
                    });

            var decisions = Subject.GetImportDecisions(_fileInfos, _artist, FilterFilesType.None, false);

            Mocker.GetMock<IAugmentingService>()
                  .Verify(c => c.Augment(It.IsAny<LocalTrack>(), It.IsAny<bool>()), Times.Exactly(_fileInfos.Count));

            decisions.Should().HaveCount(3);
            decisions.First().Rejections.Should().NotBeEmpty();
        }

        [Test]
        public void should_not_throw_if_tracks_are_not_found()
        {
            GivenSpecifications(_pass1);

            GivenAudioFiles(new []
                {
                    @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV".AsOsAgnostic(),
                    @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV".AsOsAgnostic(),
                    @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV".AsOsAgnostic()
                });

            var decisions = Subject.GetImportDecisions(_fileInfos, _artist, FilterFilesType.None, false);

            Mocker.GetMock<IAugmentingService>()
                  .Verify(c => c.Augment(It.IsAny<LocalTrack>(), It.IsAny<bool>()), Times.Exactly(_fileInfos.Count));

            decisions.Should().HaveCount(3);
            decisions.First().Rejections.Should().NotBeEmpty();
        }

        [Test]
        public void should_return_a_decision_when_exception_is_caught()
        {
            Mocker.GetMock<IAugmentingService>()
                  .Setup(c => c.Augment(It.IsAny<LocalTrack>(), It.IsAny<bool>()))
                  .Throws<TestException>();

            GivenAudioFiles(new []
                {
                    @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV".AsOsAgnostic()
                });

            Subject.GetImportDecisions(_fileInfos, _artist, FilterFilesType.None, false).Should().HaveCount(1);

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
