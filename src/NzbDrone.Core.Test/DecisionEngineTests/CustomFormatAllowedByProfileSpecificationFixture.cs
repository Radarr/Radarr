using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.CustomFormat;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class CustomFormatAllowedByProfileSpecificationFixture : CoreTest<CustomFormatAllowedbyProfileSpecification>
    {
        private RemoteMovie remoteMovie;

        private CustomFormats.CustomFormat _format1;
        private CustomFormats.CustomFormat _format2;

        [SetUp]
        public void Setup()
        {
            _format1 = new CustomFormats.CustomFormat("Awesome Format");
            _format1.Id = 1;

            _format2 = new CustomFormats.CustomFormat("Cool Format");
            _format2.Id = 2;


            var fakeSeries = Builder<Movie>.CreateNew()
                .With(c => c.Profile = new Profile { Cutoff = Quality.Bluray1080p.Id })
                         .Build();

            remoteMovie = new RemoteMovie
            {
                Movie = fakeSeries,
                ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
            };

            CustomFormatsFixture.GivenCustomFormats(CustomFormats.CustomFormat.None, _format1, _format2);
        }

        [Test]
        public void should_allow_if_format_is_defined_in_profile()
        {
            remoteMovie.ParsedMovieInfo.Quality.CustomFormats = new List<CustomFormats.CustomFormat> {_format1};
            remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name);

            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_deny_if_format_is_defined_in_profile()
        {
            remoteMovie.ParsedMovieInfo.Quality.CustomFormats = new List<CustomFormats.CustomFormat> {_format2};
            remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name);

            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_deny_if_one_format_is_defined_in_profile()
        {
            remoteMovie.ParsedMovieInfo.Quality.CustomFormats = new List<CustomFormats.CustomFormat> {_format2, _format1};
            remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name);

            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_allow_if_all_format_is_defined_in_profile()
        {
            remoteMovie.ParsedMovieInfo.Quality.CustomFormats = new List<CustomFormats.CustomFormat> {_format2, _format1};
            remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name, _format2.Name);

            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_deny_if_no_format_was_parsed_and_none_not_in_profile()
        {
            remoteMovie.ParsedMovieInfo.Quality.CustomFormats = new List<CustomFormats.CustomFormat> {};
            remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name, _format2.Name);

            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_allow_if_no_format_was_parsed_and_none_in_profile()
        {
            remoteMovie.ParsedMovieInfo.Quality.CustomFormats = new List<CustomFormats.CustomFormat> {};
            remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(CustomFormats.CustomFormat.None.Name, _format1.Name, _format2.Name);

            Subject.IsSatisfiedBy(remoteMovie, null).Accepted.Should().BeTrue();
        }
    }
}
