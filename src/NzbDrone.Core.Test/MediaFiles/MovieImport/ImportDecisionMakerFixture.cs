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
	//TODO: Add tests to ensure helpers for augmenters are correctly passed.
    public class ImportDecisionMakerFixture : CoreTest<ImportDecisionMaker>
    {
        private List<string> _videoFiles;
        private LocalMovie _localMovie;
        private Movie _movie;
        private QualityModel _quality;
        private ParsedMovieInfo _fileInfo;

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

            _movie = Builder<Movie>.CreateNew()
                                     .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                     .Build();

            _quality = new QualityModel(Quality.DVD);

            _localMovie = new LocalMovie
            {
                Movie = _movie,
                Quality = _quality,
                Path = @"C:\Test\Unsorted\The.Office.2018.DVDRip.XviD-OSiTV.avi"
            };

            _fileInfo = new ParsedMovieInfo
            {
                MovieTitle = "The Office",
                Year = 2018,
                Quality = _quality
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<ParsedMovieInfo>(), It.IsAny<Movie>(), It.IsAny<List<object>>(), It.IsAny<bool>()))
                  .Returns(_localMovie);

            Mocker.GetMock<IParsingService>()
                .Setup(c => c.ParseMinimalPathMovieInfo(It.IsAny<string>()))
                .Returns(_fileInfo);

            GivenVideoFiles(new List<string> { @"C:\Test\Unsorted\The.Office.2018.DVDRip.XviD-OSiTV.avi".AsOsAgnostic() });
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

            _fail1.Verify(c => c.IsSatisfiedBy(_localMovie, downloadClientItem), Times.Once());
            _fail2.Verify(c => c.IsSatisfiedBy(_localMovie, downloadClientItem), Times.Once());
            _fail3.Verify(c => c.IsSatisfiedBy(_localMovie, downloadClientItem), Times.Once());
            _pass1.Verify(c => c.IsSatisfiedBy(_localMovie, downloadClientItem), Times.Once());
            _pass2.Verify(c => c.IsSatisfiedBy(_localMovie, downloadClientItem), Times.Once());
            _pass3.Verify(c => c.IsSatisfiedBy(_localMovie, downloadClientItem), Times.Once());
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
                  .Setup(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<ParsedMovieInfo>(), It.IsAny<Movie>(), It.IsAny<List<object>>(), It.IsAny<bool>()))
                  .Throws<TestException>();

            _videoFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_videoFiles);

            Subject.GetImportDecisions(_videoFiles, _movie);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<ParsedMovieInfo>(), It.IsAny<Movie>(), It.IsAny<List<object>>(), It.IsAny<bool>()), Times.Exactly(_videoFiles.Count));

            ExceptionVerification.ExpectedErrors(3);
        }

        [Test]
        public void should_call_parsing_service_with_filename_as_simpletitle()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            Mocker.GetMock<IParsingService>()
                .Setup(c => c.ParseMinimalPathMovieInfo(It.IsAny<string>()))
                .Returns<ParsedMovieInfo>(null);

            var folderInfo = new ParsedMovieInfo {SimpleReleaseTitle = "A Movie Folder 2018", Quality = _quality};

            var result = Subject.GetImportDecisions(_videoFiles, _movie, null, folderInfo, true);

            var fileNames = _videoFiles.Select(System.IO.Path.GetFileName);

            Mocker.GetMock<IParsingService>()
                .Verify(
                    c => c.GetLocalMovie(It.IsAny<string>(),
                        It.Is<ParsedMovieInfo>(p => fileNames.Contains(p.SimpleReleaseTitle)), It.IsAny<Movie>(),
                        It.IsAny<List<object>>(), It.IsAny<bool>()), Times.Exactly(_videoFiles.Count));
        }

        [Test]
        public void should_use_file_quality_if_folder_quality_is_null()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var result = Subject.GetImportDecisions(_videoFiles, _movie);

            result.Single().LocalMovie.Quality.Should().Be(_fileInfo.Quality);
        }

        [Test]
        public void should_use_file_quality_if_file_quality_was_determined_by_name()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_videoFiles, _movie, null, new ParsedMovieInfo{Quality = new QualityModel(Quality.SDTV)}, true);

            result.Single().LocalMovie.Quality.Should().Be(_fileInfo.Quality);
        }

        [Test]
        public void should_use_folder_quality_when_file_quality_was_determined_by_the_extension()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            GivenVideoFiles(new string[] { @"C:\Test\Unsorted\The.Office.S03E115.mkv".AsOsAgnostic() });

            _localMovie.Path = _videoFiles.Single();
            _localMovie.Quality.QualitySource = QualitySource.Extension;
            _localMovie.Quality.Quality = Quality.HDTV720p;

            var expectedQuality = new QualityModel(Quality.SDTV);

            var result = Subject.GetImportDecisions(_videoFiles, _movie, null, new ParsedMovieInfo { Quality = expectedQuality }, true);

            result.Single().LocalMovie.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_folder_quality_when_greater_than_file_quality()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            GivenVideoFiles(new string[] { @"C:\Test\Unsorted\The.Office.S03E115.mkv".AsOsAgnostic() });

            _localMovie.Path = _videoFiles.Single();
            _localMovie.Quality.Quality = Quality.HDTV720p;

            var expectedQuality = new QualityModel(Quality.Bluray720p);

            var result = Subject.GetImportDecisions(_videoFiles, _movie, null, new ParsedMovieInfo { Quality = expectedQuality }, true);

            result.Single().LocalMovie.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_not_use_folder_quality_when_it_is_unknown()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            _movie.Profile = new Profile
                              {
                                  Items = Qualities.QualityFixture.GetDefaultQualities(Quality.DVD, Quality.Unknown)
                              };


            var folderQuality = new QualityModel(Quality.Unknown);

            var result = Subject.GetImportDecisions(_videoFiles, _movie, null, new ParsedMovieInfo { Quality = folderQuality}, true);

            result.Single().LocalMovie.Quality.Should().Be(_quality);
        }

        [Test]
        public void should_return_a_decision_when_exception_is_caught()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalMovie(It.IsAny<string>(), It.IsAny<ParsedMovieInfo>(), It.IsAny<Movie>(), It.IsAny<List<object>>(), It.IsAny<bool>()))
                  .Throws<TestException>();

            _videoFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_videoFiles);

            Subject.GetImportDecisions(_videoFiles, _movie).Should().HaveCount(1);

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
