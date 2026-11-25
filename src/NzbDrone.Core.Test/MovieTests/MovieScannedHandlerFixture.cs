using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests
{
    public class MovieScannedHandlerFixture : CoreTest<MovieScannedHandler>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                    .With(m => m.Path = "C:\\Movies\\Wrong Name".AsOsAgnostic())
                                    .With(m => m.AddOptions = new AddMovieOptions
                                    {
                                        Monitor = MonitorTypes.MovieOnly
                                    })
                                    .Build();
        }

        [Test]
        public void should_do_nothing_when_add_options_is_null()
        {
            _movie.AddOptions = null;

            Subject.Handle(new MovieScannedEvent(_movie, new List<string>()));

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.RemoveAddOptions(It.IsAny<Movie>()), Times.Never());
        }

        [Test]
        public void should_clear_add_options_after_handling()
        {
            Subject.Handle(new MovieScannedEvent(_movie, new List<string>()));

            _movie.AddOptions.Should().BeNull();

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.RemoveAddOptions(_movie), Times.Once());
        }

        [Test]
        public void should_clear_add_options_on_scan_skipped_event_too()
        {
            Subject.Handle(new MovieScanSkippedEvent(_movie, MovieScanSkippedReason.RootFolderIsEmpty));

            _movie.AddOptions.Should().BeNull();

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.RemoveAddOptions(_movie), Times.Once());
        }
    }
}
