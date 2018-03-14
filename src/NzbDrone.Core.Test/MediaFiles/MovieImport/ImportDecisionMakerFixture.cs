using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Test.Common;
using FizzWare.NBuilder;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport
{
    [TestFixture]
	//TODO: Update all of this for movies.
    public class ImportDecisionMakerFixture : CoreTest<ImportDecisionMaker>
    {
        private List<string> _videoFiles;
        private LocalMovie _localEpisode;
        private Movie _series;
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

            _pass1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalMovie>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Accept());
            _pass2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalMovie>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Accept());
            _pass3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalMovie>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Accept());

            _fail1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalMovie>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Reject("_fail1"));
            _fail2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalMovie>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Reject("_fail2"));
            _fail3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalMovie>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Reject("_fail3"));

            _series = Builder<Movie>.CreateNew()
                                     .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                     .Build();

            _quality = new QualityModel(Quality.DVD);

            _localEpisode = new LocalMovie
            { 
                Movie = _series,
                Quality = _quality,
                Path = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV.avi"
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<ParsedMovieInfo>(), It.IsAny<bool>()))
                  .Returns(_localEpisode);

            GivenVideoFiles(new List<string> { @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV.avi".AsOsAgnostic() });
        }

        private void GivenSpecifications(params Mock<IImportDecisionEngineSpecification>[] mocks)
        {
            Mocker.SetConstant(mocks.Select(c => c.Object));
        }

        private void GivenVideoFiles(IEnumerable<string> videoFiles)
        {
            _videoFiles = videoFiles.ToList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.FilterExistingFiles(_videoFiles, It.IsAny<Movie>()))
                  .Returns(_videoFiles);
        }

        [Test]
        public void should_call_all_specifications()
        {
            var downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            Subject.GetImportDecisions(_videoFiles, new Movie(), downloadClientItem, null, false);

            _fail1.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
            _fail2.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
            _fail3.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
            _pass1.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
            _pass2.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
            _pass3.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
        }

        [Test]
        public void should_return_rejected_if_single_specs_fail()
        {
            GivenSpecifications(_fail1);

            var result = Subject.GetImportDecisions(_videoFiles, new Movie());

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_rejected_if_one_of_specs_fail()
        {
            GivenSpecifications(_pass1, _fail1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_videoFiles, new Movie());

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_pass_if_all_specs_pass()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_videoFiles, new Movie());

            result.Single().Approved.Should().BeTrue();
        }

        [Test]
        public void should_have_same_number_of_rejections_as_specs_that_failed()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            var result = Subject.GetImportDecisions(_videoFiles, new Movie());
            result.Single().Rejections.Should().HaveCount(3);
        }

        [Test]
        public void should_not_blowup_the_process_due_to_failed_parse()
        {
            GivenSpecifications(_pass1);

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<ParsedMovieInfo>(), It.IsAny<bool>()))
                  .Throws<TestException>();

            _videoFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_videoFiles);

            Subject.GetImportDecisions(_videoFiles, _series);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<ParsedMovieInfo>(), It.IsAny<bool>()), Times.Exactly(_videoFiles.Count));

            ExceptionVerification.ExpectedErrors(3);
        }

        [Test]
        public void should_use_file_quality_if_folder_quality_is_null()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var expectedQuality = QualityParser.ParseQuality(_videoFiles.Single());

            var result = Subject.GetImportDecisions(_videoFiles, _series);

            result.Single().LocalMovie.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_file_quality_if_file_quality_was_determined_by_name()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var expectedQuality = QualityParser.ParseQuality(_videoFiles.Single());

            var result = Subject.GetImportDecisions(_videoFiles, _series, null, new ParsedMovieInfo{Quality = new QualityModel(Quality.SDTV)}, true);

            result.Single().LocalMovie.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_folder_quality_when_file_quality_was_determined_by_the_extension()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            GivenVideoFiles(new string[] { @"C:\Test\Unsorted\The.Office.S03E115.mkv".AsOsAgnostic() });

            _localEpisode.Path = _videoFiles.Single();
            _localEpisode.Quality.QualitySource = QualitySource.Extension;
            _localEpisode.Quality.Quality = Quality.HDTV720p;

            var expectedQuality = new QualityModel(Quality.SDTV);

            var result = Subject.GetImportDecisions(_videoFiles, _series, null, new ParsedMovieInfo { Quality = expectedQuality }, true);

            result.Single().LocalMovie.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_folder_quality_when_greater_than_file_quality()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            GivenVideoFiles(new string[] { @"C:\Test\Unsorted\The.Office.S03E115.mkv".AsOsAgnostic() });

            _localEpisode.Path = _videoFiles.Single();
            _localEpisode.Quality.Quality = Quality.HDTV720p;

            var expectedQuality = new QualityModel(Quality.Bluray720p);

            var result = Subject.GetImportDecisions(_videoFiles, _series, null, new ParsedMovieInfo { Quality = expectedQuality }, true);

            result.Single().LocalMovie.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_not_throw_if_episodes_are_not_found()
        {
            GivenSpecifications(_pass1);

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<ParsedMovieInfo>(), It.IsAny<bool>()))
                  .Returns(new LocalMovie() { Path = "test" });

            _videoFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_videoFiles);

            var decisions = Subject.GetImportDecisions(_videoFiles, _series);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<ParsedMovieInfo>(), It.IsAny<bool>()), Times.Exactly(_videoFiles.Count));

            decisions.Should().HaveCount(3);
            decisions.First().Rejections.Should().NotBeEmpty();
        }

        [Test]
        public void should_not_use_folder_for_full_season()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Movie.Title.S01\S01E01.mkv".AsOsAgnostic(),
                                 @"C:\Test\Unsorted\Movie.Title.S01\S01E02.mkv".AsOsAgnostic(),
                                 @"C:\Test\Unsorted\Movie.Title.S01\S01E03.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseMovieTitle("Movie.Title.S01", false);

            Subject.GetImportDecisions(_videoFiles, _series, null, folderInfo, true);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), null, true), Times.Exactly(3));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), It.Is<ParsedMovieInfo>(p => p != null), true), Times.Never());
        }

        [Test]
        public void should_not_use_folder_when_it_contains_more_than_one_valid_video_file()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Movie.Title.S01E01\S01E01.mkv".AsOsAgnostic(),
                                 @"C:\Test\Unsorted\Movie.Title.S01E01\1x01.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseMovieTitle("Movie.Title.S01E01", false);

            Subject.GetImportDecisions(_videoFiles, _series, null, folderInfo, true);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), null, true), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), It.Is<ParsedMovieInfo>(p => p != null), true), Times.Never());
        }

        [Test]
        public void should_use_folder_when_only_one_video_file()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Movie.Title.S01E01\S01E01.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseMovieTitle("Movie.Title.S01E01", false);

            Subject.GetImportDecisions(_videoFiles, _series, null, folderInfo, true);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<ParsedMovieInfo>(), true), Times.Exactly(1));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), null, true), Times.Never());
        }

        [Test]
        public void should_use_folder_when_only_one_video_file_and_a_sample()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Movie.Title.S01E01\S01E01.mkv".AsOsAgnostic(),
                                 @"C:\Test\Unsorted\Movie.Title.S01E01\S01E01.sample.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles.ToList());

            Mocker.GetMock<IDetectSample>()
                  .Setup(s => s.IsSample(_series, It.IsAny<QualityModel>(), It.Is<string>(c => c.Contains("sample")), It.IsAny<long>(), It.IsAny<bool>()))
                  .Returns(true);

            var folderInfo = Parser.Parser.ParseMovieTitle("Movie.Title.S01E01", false);

            Subject.GetImportDecisions(_videoFiles, _series, null, folderInfo, true);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<ParsedMovieInfo>(), true), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), null, true), Times.Never());
        }

        [Test]
		[Ignore("Movie")]
        public void should_not_use_folder_name_if_file_name_is_scene_name()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Movie.Title.S01E01.720p.HDTV-LOL\Movie.Title.S01E01.720p.HDTV-LOL.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseMovieTitle("Movie.Title.S01E01.720p.HDTV-LOL", false);

            Subject.GetImportDecisions(_videoFiles, _series, null, folderInfo, true);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), null, true), Times.Exactly(1));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), It.Is<ParsedMovieInfo>(p => p != null), true), Times.Never());
        }

        [Test]
        public void should_not_use_folder_quality_when_it_is_unknown()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            _series.Profile = new Profile
                              {
                                  Items = Qualities.QualityFixture.GetDefaultQualities(Quality.DVD, Quality.Unknown)
                              };


            var folderQuality = new QualityModel(Quality.Unknown);

            var result = Subject.GetImportDecisions(_videoFiles, _series, null, new ParsedMovieInfo { Quality = folderQuality}, true);

            result.Single().LocalMovie.Quality.Should().Be(_quality);
        }

        [Test]
        public void should_return_a_decision_when_exception_is_caught()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<ParsedMovieInfo>(), It.IsAny<bool>()))
                  .Throws<TestException>();

            _videoFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_videoFiles);

            Subject.GetImportDecisions(_videoFiles, _series).Should().HaveCount(1);

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
