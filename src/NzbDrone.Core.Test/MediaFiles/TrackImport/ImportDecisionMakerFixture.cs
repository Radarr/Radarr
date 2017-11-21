using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;
using FizzWare.NBuilder;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Core.Test.MediaFiles.TrackImport
{
    [TestFixture]
    public class ImportDecisionMakerFixture : CoreTest<ImportDecisionMaker>
    {
        private List<string> _audioFiles;
        private LocalTrack _localTrack;
        private Artist _artist;
        private QualityModel _quality;

        private Mock<IImportDecisionEngineSpecification> _pass1;
        private Mock<IImportDecisionEngineSpecification> _pass2;
        private Mock<IImportDecisionEngineSpecification> _pass3;

        private Mock<IImportDecisionEngineSpecification> _fail1;
        private Mock<IImportDecisionEngineSpecification> _fail2;
        private Mock<IImportDecisionEngineSpecification> _fail3;

        [SetUp]
        public void Setup()
        {
            _pass1 = new Mock<IImportDecisionEngineSpecification>();
            _pass2 = new Mock<IImportDecisionEngineSpecification>();
            _pass3 = new Mock<IImportDecisionEngineSpecification>();

            _fail1 = new Mock<IImportDecisionEngineSpecification>();
            _fail2 = new Mock<IImportDecisionEngineSpecification>();
            _fail3 = new Mock<IImportDecisionEngineSpecification>();

            _pass1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Accept());
            _pass2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Accept());
            _pass3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Accept());

            _fail1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Reject("_fail1"));
            _fail2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Reject("_fail2"));
            _fail3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalTrack>())).Returns(Decision.Reject("_fail3"));

            _artist = Builder<Artist>.CreateNew()
                                     .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                     .With(e => e.LanguageProfile = new LanguageProfile { Languages = Languages.LanguageFixture.GetDefaultLanguages() })
                                     .Build();

            _quality = new QualityModel(Quality.MP3_256);

            _localTrack = new LocalTrack
            { 
                Artist = _artist,
                Quality = _quality,
                Language = Language.Spanish,
                Tracks = new List<Track> { new Track() },
                Path = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.Spanish.XviD-OSiTV.avi"
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), It.IsAny<ParsedTrackInfo>()))
                  .Returns(_localTrack);

            GivenVideoFiles(new List<string> { @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.Spanish.XviD-OSiTV.avi".AsOsAgnostic() });
        }

        private void GivenSpecifications(params Mock<IImportDecisionEngineSpecification>[] mocks)
        {
            Mocker.SetConstant(mocks.Select(c => c.Object));
        }

        private void GivenVideoFiles(IEnumerable<string> videoFiles)
        {
            _audioFiles = videoFiles.ToList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.FilterExistingFiles(_audioFiles, It.IsAny<Artist>()))
                  .Returns(_audioFiles);
        }

        [Test]
        public void should_call_all_specifications()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            Subject.GetImportDecisions(_audioFiles, new Artist(), null);

            _fail1.Verify(c => c.IsSatisfiedBy(_localTrack), Times.Once());
            _fail2.Verify(c => c.IsSatisfiedBy(_localTrack), Times.Once());
            _fail3.Verify(c => c.IsSatisfiedBy(_localTrack), Times.Once());
            _pass1.Verify(c => c.IsSatisfiedBy(_localTrack), Times.Once());
            _pass2.Verify(c => c.IsSatisfiedBy(_localTrack), Times.Once());
            _pass3.Verify(c => c.IsSatisfiedBy(_localTrack), Times.Once());
        }

        [Test]
        public void should_return_rejected_if_single_specs_fail()
        {
            GivenSpecifications(_fail1);

            var result = Subject.GetImportDecisions(_audioFiles, new Artist());

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_rejected_if_one_of_specs_fail()
        {
            GivenSpecifications(_pass1, _fail1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_audioFiles, new Artist());

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_pass_if_all_specs_pass()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_audioFiles, new Artist());

            result.Single().Approved.Should().BeTrue();
        }

        [Test]
        public void should_have_same_number_of_rejections_as_specs_that_failed()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            var result = Subject.GetImportDecisions(_audioFiles, new Artist());
            result.Single().Rejections.Should().HaveCount(3);
        }

        [Test]
        public void should_not_blowup_the_process_due_to_failed_parse()
        {
            GivenSpecifications(_pass1);

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), It.IsAny<ParsedTrackInfo>()))
                  .Throws<TestException>();

            _audioFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_audioFiles);

            Subject.GetImportDecisions(_audioFiles, _artist);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), It.IsAny<ParsedTrackInfo>()), Times.Exactly(_audioFiles.Count));

            ExceptionVerification.ExpectedErrors(3);
        }

        [Test]
        public void should_use_file_quality_if_folder_quality_is_null()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var expectedQuality = QualityParser.ParseQuality(_audioFiles.Single(), null, 0);

            var result = Subject.GetImportDecisions(_audioFiles, _artist);
            
            result.Single().LocalTrack.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_file_language_if_folder_language_is_null()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var expectedLanguage = LanguageParser.ParseLanguage(_audioFiles.Single());

            var result = Subject.GetImportDecisions(_audioFiles, _artist);

            result.Single().LocalTrack.Language.Should().Be(expectedLanguage);
        }

    [Test]
        public void should_use_file_quality_if_file_quality_was_determined_by_name()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var expectedQuality = QualityParser.ParseQuality(_audioFiles.Single(), null, 0);

            var result = Subject.GetImportDecisions(_audioFiles, _artist, new ParsedTrackInfo{Quality = new QualityModel(Quality.MP3_256) });

            result.Single().LocalTrack.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_folder_quality_when_file_quality_was_determined_by_the_extension()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            GivenVideoFiles(new string[] { @"C:\Test\Unsorted\The.Office.S03E115.mkv".AsOsAgnostic() });

            _localTrack.Path = _audioFiles.Single();
            _localTrack.Quality.QualitySource = QualitySource.Extension;
            _localTrack.Quality.Quality = Quality.MP3_256;

            var expectedQuality = new QualityModel(Quality.MP3_256);

            var result = Subject.GetImportDecisions(_audioFiles, _artist, new ParsedTrackInfo { Quality = expectedQuality });

            result.Single().LocalTrack.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_folder_quality_when_greater_than_file_quality()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            GivenVideoFiles(new string[] { @"C:\Test\Unsorted\The.Office.S03E115.mkv".AsOsAgnostic() });

            _localTrack.Path = _audioFiles.Single();
            _localTrack.Quality.Quality = Quality.MP3_256;

            var expectedQuality = new QualityModel(Quality.MP3_256);

            var result = Subject.GetImportDecisions(_audioFiles, _artist, new ParsedTrackInfo { Quality = expectedQuality });

            result.Single().LocalTrack.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_folder_language_when_greater_than_file_language()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            GivenVideoFiles(new string[] { @"C:\Test\Unsorted\The.Office.S03E115.Spanish.mkv".AsOsAgnostic() });

            _localTrack.Path = _audioFiles.Single();
            _localTrack.Quality.Quality = Quality.MP3_320;
            _localTrack.Language = Language.Spanish;

            var expectedLanguage = Language.French;

            var result = Subject.GetImportDecisions(_audioFiles, _artist, new ParsedTrackInfo { Language = expectedLanguage, Quality = new QualityModel(Quality.MP3_192) });

            result.Single().LocalTrack.Language.Should().Be(expectedLanguage);
        }

[Test]
        public void should_not_throw_if_episodes_are_not_found()
        {
            GivenSpecifications(_pass1);

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), It.IsAny<ParsedTrackInfo>()))
                  .Returns(new LocalTrack() { Path = "test" });

            _audioFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_audioFiles);

            var decisions = Subject.GetImportDecisions(_audioFiles, _artist);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), It.IsAny<ParsedTrackInfo>()), Times.Exactly(_audioFiles.Count));

            decisions.Should().HaveCount(3);
            decisions.First().Rejections.Should().NotBeEmpty();
        }

        [Test]
        public void should_not_use_folder_for_full_season()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Artist.Title.S01\S01E01.mkv".AsOsAgnostic(),
                                 @"C:\Test\Unsorted\Artist.Title.S01\S01E02.mkv".AsOsAgnostic(),
                                 @"C:\Test\Unsorted\Artist.Title.S01\S01E03.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseMusicTitle("Artist.Title.S01");

            Subject.GetImportDecisions(_audioFiles, _artist, folderInfo);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), null), Times.Exactly(3));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), It.Is<ParsedTrackInfo>(p => p != null)), Times.Never());
        }

        [Test]
        public void should_not_use_folder_when_it_contains_more_than_one_valid_video_file()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Artist.Title.S01E01\S01E01.mkv".AsOsAgnostic(),
                                 @"C:\Test\Unsorted\Artist.Title.S01E01\1x01.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseMusicTitle("Artist.Title.S01E01");

            Subject.GetImportDecisions(_audioFiles, _artist, folderInfo);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), null), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), It.Is<ParsedTrackInfo>(p => p != null)), Times.Never());
        }

        [Test]
        public void should_use_folder_when_only_one_video_file()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Artist.Title.S01E01\S01E01.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseMusicTitle("Artist.Title.S01E01");

            Subject.GetImportDecisions(_audioFiles, _artist, folderInfo);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), It.IsAny<ParsedTrackInfo>()), Times.Exactly(1));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), null), Times.Never());
        }

        [Test]
        public void should_not_use_folder_name_if_file_name_is_scene_name()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Artist.Title.S01E01.720p.HDTV-LOL\Artist.Title.S01E01.720p.HDTV-LOL.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseMusicTitle("Artist.Title.S01E01.720p.HDTV-LOL");

            Subject.GetImportDecisions(_audioFiles, _artist, folderInfo);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), null), Times.Exactly(1));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), It.Is<ParsedTrackInfo>(p => p != null)), Times.Never());
        }

        [Test]
        public void should_not_use_folder_quality_when_it_is_unknown()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            _artist.Profile = new Profile
                              {
                                  Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3_256, Quality.Unknown)
                              };


            var folderQuality = new QualityModel(Quality.Unknown);

            var result = Subject.GetImportDecisions(_audioFiles, _artist, new ParsedTrackInfo { Quality = folderQuality});

            result.Single().LocalTrack.Quality.Should().Be(_quality);
        }

        [Test]
        public void should_return_a_decision_when_exception_is_caught()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalTrack(It.IsAny<string>(), It.IsAny<Artist>(), It.IsAny<ParsedTrackInfo>()))
                  .Throws<TestException>();

            _audioFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_audioFiles);

            Subject.GetImportDecisions(_audioFiles, _artist).Should().HaveCount(1);

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
