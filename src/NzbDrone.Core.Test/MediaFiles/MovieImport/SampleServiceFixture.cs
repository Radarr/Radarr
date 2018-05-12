using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport
{
    [TestFixture]
    public class SampleServiceFixture : CoreTest<DetectSample>
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

        private void GivenFileSize(long size)
        {
            _localMovie.Size = size;
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

            ShouldBeFalse();

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_for_strm()
        {
            _localMovie.Path = @"C:\Test\some.show.s01e01.strm";

            ShouldBeFalse();

            Mocker.GetMock<IVideoFileInfoReader>().Verify(c => c.GetRunTime(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_runtime()
        {
            GivenRuntime(120);
            GivenFileSize(1000.Megabytes());

            Subject.IsSample(_localMovie.Movie,
                             _localMovie.Quality,
                             _localMovie.Path,
                             _localMovie.Size,
                             false);

            Mocker.GetMock<IVideoFileInfoReader>().Verify(v => v.GetRunTime(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_return_true_if_runtime_is_less_than_minimum()
        {
            GivenRuntime(60);

            ShouldBeTrue();
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_minimum()
        {
            GivenRuntime(600);

            ShouldBeFalse();
        }

        [Test]
        public void should_return_false_if_runtime_greater_than_webisode_minimum()
        {
            _movie.Runtime = 6;
            GivenRuntime(299);

            ShouldBeFalse();
        }

        [Test]
        public void should_fall_back_to_file_size_if_mediainfo_dll_not_found_acceptable_size()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Throws<DllNotFoundException>();

            GivenFileSize(1000.Megabytes());
            ShouldBeFalse();
        }

        [Test]
        public void should_fall_back_to_file_size_if_mediainfo_dll_not_found_undersize()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(s => s.GetRunTime(It.IsAny<string>()))
                  .Throws<DllNotFoundException>();

            GivenFileSize(1.Megabytes());
            ShouldBeTrue();
        }





        private void ShouldBeTrue()
        {
            Subject.IsSample(_localMovie.Movie,
                                         _localMovie.Quality,
                                         _localMovie.Path,
                                         _localMovie.Size,
                                         false).Should().BeTrue();
        }

        private void ShouldBeFalse()
        {
            Subject.IsSample(_localMovie.Movie,
                             _localMovie.Quality,
                             _localMovie.Path,
                             _localMovie.Size,
                             false).Should().BeFalse();
        }
    }
}
