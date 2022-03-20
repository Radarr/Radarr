using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests
{
    [TestFixture]
    public class MovieIsAvailableFixture : CoreTest
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .Build();
        }

        private void SetMovieProperties(DateTime? cinema, DateTime? physical, DateTime? digital, MovieStatusType minimumAvailability)
        {
            _movie.MovieMetadata.Value.InCinemas = cinema;
            _movie.MovieMetadata.Value.PhysicalRelease = physical;
            _movie.MovieMetadata.Value.DigitalRelease = digital;
            _movie.MinimumAvailability = minimumAvailability;
        }

        //minAvail = TBA
        [TestCase(null, null, null, MovieStatusType.TBA, true)]
        [TestCase("2000/01/01 21:10:42", null, null, MovieStatusType.TBA, true)]
        [TestCase("2100/01/01 21:10:42", null, null, MovieStatusType.TBA, true)]
        [TestCase(null, "2000/01/01 21:10:42", null, MovieStatusType.TBA, true)]
        [TestCase(null, "2100/01/01 21:10:42", null, MovieStatusType.TBA, true)]
        [TestCase(null, null, "2000/01/01 21:10:42", MovieStatusType.TBA, true)]
        [TestCase(null, null, "2100/01/01 21:10:42", MovieStatusType.TBA, true)]

        //minAvail = Announced
        [TestCase(null, null, null, MovieStatusType.Announced, true)]
        [TestCase("2000/01/01 21:10:42", null, null, MovieStatusType.Announced, true)]
        [TestCase("2100/01/01 21:10:42", null, null, MovieStatusType.Announced, true)]
        [TestCase(null, "2000/01/01 21:10:42", null, MovieStatusType.Announced, true)]
        [TestCase(null, "2100/01/01 21:10:42", null, MovieStatusType.Announced, true)]
        [TestCase(null, null, "2000/01/01 21:10:42", MovieStatusType.Announced, true)]
        [TestCase(null, null, "2100/01/01 21:10:42", MovieStatusType.Announced, true)]

        //minAvail = InCinemas
        //InCinemas is known and in the past others are not known or in future
        [TestCase("2000/01/01 21:10:42", null, null, MovieStatusType.InCinemas, true)]
        [TestCase("2000/01/01 21:10:42", "2100/01/01 21:10:42", null, MovieStatusType.InCinemas, true)]
        [TestCase("2000/01/01 21:10:42", "2100/01/01 21:10:42", "2100/01/01 21:10:42", MovieStatusType.InCinemas, true)]
        [TestCase("2000/01/01 21:10:42", null, "2100/01/01 21:10:42", MovieStatusType.InCinemas, true)]

        //InCinemas is known and in the future others are not known or in future
        [TestCase("2100/01/01 21:10:42", null, null, MovieStatusType.InCinemas, false)]
        [TestCase("2100/01/01 21:10:42", "2100/01/01 21:10:42", null, MovieStatusType.InCinemas, false)]
        [TestCase("2100/01/01 21:10:42", "2100/01/01 21:10:42", "2100/01/01 21:10:42", MovieStatusType.InCinemas, false)]
        [TestCase("2100/01/01 21:10:42", null, "2100/01/01 21:10:42", MovieStatusType.InCinemas, false)]

        //handle the cases where InCinemas date is not known but Digital/Physical are and passed -- this refers to the issue being fixed along with these tests
        [TestCase(null, "2000/01/01 21:10:42", null, MovieStatusType.InCinemas, true)]
        [TestCase(null, "2000/01/01 21:10:42", "2000/01/01 21:10:42", MovieStatusType.InCinemas, true)]
        [TestCase(null, null, "2000/01/01 21:10:42", MovieStatusType.InCinemas, true)]

        //same as previous but digital/physical are in future
        [TestCase(null, "2100/01/01 21:10:42", null, MovieStatusType.InCinemas, false)]
        [TestCase(null, "2100/01/01 21:10:42", "2100/01/01 21:10:42", MovieStatusType.InCinemas, false)]
        [TestCase(null, null, "2100/01/01 21:10:42", MovieStatusType.InCinemas, false)]

        //no date values
        [TestCase(null, null, null, MovieStatusType.InCinemas, false)]

        //minAvail = Released
        [TestCase(null, null, null, MovieStatusType.Released, false)]
        [TestCase("2000/01/01 21:10:42", null, null, MovieStatusType.Released, true)]
        [TestCase("2100/01/01 21:10:42", null, null, MovieStatusType.Released, false)]
        [TestCase(null, "2000/01/01 21:10:42", null, MovieStatusType.Released, true)]
        [TestCase(null, "2100/01/01 21:10:42", null, MovieStatusType.Released, false)]
        [TestCase(null, null, "2000/01/01 21:10:42", MovieStatusType.Released, true)]
        [TestCase(null, null, "2100/01/01 21:10:42", MovieStatusType.Released, false)]
        public void should_have_correct_availability(DateTime? cinema, DateTime? physical, DateTime? digital, MovieStatusType minimumAvailability, bool result)
        {
            SetMovieProperties(cinema, physical, digital, minimumAvailability);
            _movie.IsAvailable().Should().Be(result);
        }

        [Test]
        public void positive_delay_should_effect_availability()
        {
            SetMovieProperties(null, DateTime.Now.AddDays(-5), null, MovieStatusType.Released);
            _movie.IsAvailable().Should().BeTrue();
            _movie.IsAvailable(6).Should().BeFalse();
        }

        [Test]
        public void negative_delay_should_effect_availability()
        {
            SetMovieProperties(null, DateTime.Now.AddDays(5), null, MovieStatusType.Released);
            _movie.IsAvailable().Should().BeFalse();
            _movie.IsAvailable(-6).Should().BeTrue();
        }

        [Test]
        public void minimum_availability_released_no_date_but_ninety_days_after_cinemas()
        {
            SetMovieProperties(DateTime.Now.AddDays(-91), null, null, MovieStatusType.Released);
            _movie.IsAvailable().Should().BeTrue();
            SetMovieProperties(DateTime.Now.AddDays(-89), null, null, MovieStatusType.Released);
            _movie.IsAvailable().Should().BeFalse();
            SetMovieProperties(DateTime.Now.AddDays(-89), DateTime.Now.AddDays(-40), null, MovieStatusType.Released);
            _movie.IsAvailable().Should().BeTrue();
            SetMovieProperties(DateTime.Now.AddDays(-91), DateTime.Now.AddDays(40), null, MovieStatusType.Released);
            _movie.IsAvailable().Should().BeFalse();
        }
    }
}
