using System;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport
{
    [TestFixture]
    public class DetectSampleFixture : CoreTest<DetectSample>
    {
        private Movie _movie;
        private LocalMovie _localMovie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Runtime = 30)
                                     .Build();

            _localMovie = new LocalMovie
            {
                Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                Movie = _movie,
                Quality = new QualityModel(Quality.HDTV720p)
            };
        }

        private void GivenRuntime(int seconds)
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Returns(new TimeSpan(0, 0, seconds));
        }

        [Test]
        public void should_return_false_for_flv()
        {
            _localMovie.Path = @"C:\Test\some.show.s01e01.flv";

            ShouldBeNotSample();

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_for_strm()
        {
            _localMovie.Path = @"C:\Test\some.show.s01e01.strm";

            ShouldBeNotSample();

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_runtime()
        {
            GivenRuntime(120);

            Subject.IsSample(_localMovie.Movie,
                             _localMovie.Path,
                             false);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(v => v.GetRunTime(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_return_true_if_runtime_is_less_than_minimum()
        {
            GivenRuntime(60);

            ShouldBeSample();
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_minimum()
        {
            GivenRuntime(600);

            ShouldBeNotSample();
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_webisode_minimum()
        {
            _movie.Runtime = 6;
            GivenRuntime(299);

            ShouldBeNotSample();
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_anime_short_minimum()
        {
            _movie.Runtime = 2;
            GivenRuntime(60);

            ShouldBeNotSample();
        }

        [Test]
        public void should_return_true_if_runtime_less_than_anime_short_minimum()
        {
            _movie.Runtime = 2;
            GivenRuntime(10);

            ShouldBeSample();
        }

        [Test]
        public void should_return_indeterminate_if_mediainfo_result_is_null()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Returns((TimeSpan?)null);

            Subject.IsSample(_localMovie.Movie,
                             _localMovie.Path,
                             false).Should().Be(DetectSampleResult.Indeterminate);

            ExceptionVerification.ExpectedErrors(1);
        }

        private void ShouldBeSample()
        {
            Subject.IsSample(_localMovie.Movie,
                             _localMovie.Path,
                             false).Should().Be(DetectSampleResult.Sample);
        }

        private void ShouldBeNotSample()
        {
            Subject.IsSample(_localMovie.Movie,
                             _localMovie.Path,
                             false).Should().Be(DetectSampleResult.NotSample);
        }
    }
}
