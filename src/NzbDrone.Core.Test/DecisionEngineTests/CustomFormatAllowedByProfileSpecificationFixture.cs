using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class CustomFormatAllowedByProfileSpecificationFixture : CoreTest<CustomFormatAllowedbyProfileSpecification>
    {
        private RemoteMovie _remoteMovie;

        private CustomFormat _format1;
        private CustomFormat _format2;

        [SetUp]
        public void Setup()
        {
            _format1 = new CustomFormat("Awesome Format");
            _format1.Id = 1;

            _format2 = new CustomFormat("Cool Format");
            _format2.Id = 2;

            var fakeSeries = Builder<Movie>.CreateNew()
                .With(c => c.Profile = new Profile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    MinFormatScore = 1
                })
                .Build();

            _remoteMovie = new RemoteMovie
            {
                Movie = fakeSeries,
                ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
            };

            CustomFormatsFixture.GivenCustomFormats(_format1, _format2);
        }

        [Test]
        public void should_allow_if_format_score_greater_than_min()
        {
            _remoteMovie.CustomFormats = new List<CustomFormat> { _format1 };
            _remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name);
            _remoteMovie.CustomFormatScore = _remoteMovie.Movie.Profile.CalculateCustomFormatScore(_remoteMovie.CustomFormats);

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_deny_if_format_score_not_greater_than_min()
        {
            _remoteMovie.CustomFormats = new List<CustomFormat> { _format2 };
            _remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name);
            _remoteMovie.CustomFormatScore = _remoteMovie.Movie.Profile.CalculateCustomFormatScore(_remoteMovie.CustomFormats);

            Console.WriteLine(_remoteMovie.CustomFormatScore);
            Console.WriteLine(_remoteMovie.Movie.Profile.MinFormatScore);

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_deny_if_format_score_not_greater_than_min_2()
        {
            _remoteMovie.CustomFormats = new List<CustomFormat> { _format2, _format1 };
            _remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name);
            _remoteMovie.CustomFormatScore = _remoteMovie.Movie.Profile.CalculateCustomFormatScore(_remoteMovie.CustomFormats);

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_allow_if_all_format_is_defined_in_profile()
        {
            _remoteMovie.CustomFormats = new List<CustomFormat> { _format2, _format1 };
            _remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name, _format2.Name);
            _remoteMovie.CustomFormatScore = _remoteMovie.Movie.Profile.CalculateCustomFormatScore(_remoteMovie.CustomFormats);

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_deny_if_no_format_was_parsed_and_min_score_positive()
        {
            _remoteMovie.CustomFormats = new List<CustomFormat> { };
            _remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name, _format2.Name);
            _remoteMovie.CustomFormatScore = _remoteMovie.Movie.Profile.CalculateCustomFormatScore(_remoteMovie.CustomFormats);

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_allow_if_no_format_was_parsed_min_score_is_zero()
        {
            _remoteMovie.CustomFormats = new List<CustomFormat> { };
            _remoteMovie.Movie.Profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(_format1.Name, _format2.Name);
            _remoteMovie.Movie.Profile.MinFormatScore = 0;
            _remoteMovie.CustomFormatScore = _remoteMovie.Movie.Profile.CalculateCustomFormatScore(_remoteMovie.CustomFormats);

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }
    }
}
