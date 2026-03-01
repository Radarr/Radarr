using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport
{
    [TestFixture]
    public class LocalMovieCustomFormatCalculationServiceFixture : CoreTest<LocalMovieCustomFormatCalculationService>
    {
        private const int EnglishCustomFormatScore = 10;
        private const int SpanishCustomFormatScore = 2;
        private LocalMovie _localMovie;
        private Movie _movie;
        private QualityModel _quality;
        private CustomFormat _englishCustomFormat;
        private CustomFormat _spanishCustomFormat;

        [SetUp]
        public void Setup()
        {
            _englishCustomFormat = new CustomFormat("HasEnglish") { Id = 1 };
            _spanishCustomFormat = new CustomFormat("HasSpanish") { Id = 2 };
            _movie = Builder<Movie>.CreateNew()
                                     .With(e => e.Path = @"C:\Test\Movie".AsOsAgnostic())
                                     .With(e => e.QualityProfile = new QualityProfile
                                     {
                                         Items = Qualities.QualityFixture.GetDefaultQualities(),
                                         FormatItems = [
                                             new ProfileFormatItem { Format = _englishCustomFormat, Score = EnglishCustomFormatScore },
                                             new ProfileFormatItem { Format = _spanishCustomFormat, Score = SpanishCustomFormatScore }
                                         ]
                                     })
                                     .Build();

            _quality = new QualityModel(Quality.DVD);

            _localMovie = new LocalMovie
            {
                Movie = _movie,
                Quality = _quality,
                Languages = new List<Language> { Language.Spanish },
                Path = @"C:\Test\Unsorted\The.Movie.2026.DVDRip.Spanish.XviD-OSiTV.avi"
            };

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(It.IsAny<LocalMovie>(), It.Is<string>(x => x.Contains("English"))))
                .Returns([_englishCustomFormat]);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(It.IsAny<LocalMovie>(), It.Is<string>(x => x.Contains("Spanish"))))
                .Returns([_spanishCustomFormat]);
        }

        [Test]
        public void should_build_a_filename_and_use_it_to_calculate_custom_score()
        {
            var renamedFileName = @"C:\Test\Unsorted\The.Movie.2026.DVDRip.English.XviD-OSiTV.avi";

            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.BuildFileName(It.IsAny<Movie>(), It.IsAny<MovieFile>(), null, null))
                .Returns(renamedFileName);

            Subject.ParseMovieCustomFormats(_localMovie).Should().BeEquivalentTo([_englishCustomFormat]);
        }

        [Test]
        public void should_update_custom_formats_on_local_movie()
        {
            var renamedFileName = @"C:\Test\Unsorted\The.Movie.2026.DVDRip.English.XviD-OSiTV.avi";

            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.BuildFileName(It.IsAny<Movie>(), It.IsAny<MovieFile>(), null, null))
                .Returns(renamedFileName);

            Subject.UpdateMovieCustomFormats(_localMovie);
            _localMovie.FileNameUsedForCustomFormatCalculation.Should().Be(renamedFileName);

            _localMovie.OriginalFileNameCustomFormats.Should().BeEquivalentTo([_spanishCustomFormat]);
            _localMovie.OriginalFileNameCustomFormatScore.Should().Be(SpanishCustomFormatScore);

            _localMovie.CustomFormats.Should().BeEquivalentTo([_englishCustomFormat]);
            _localMovie.CustomFormatScore.Should().Be(EnglishCustomFormatScore);
        }
    }
}
