using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class LanguageSpecificationFixture : CoreTest
    {
        private RemoteMovie _remoteMovie;

        [SetUp]
        public void Setup()
        {
            _remoteMovie = new RemoteMovie
            {
                ParsedMovieInfo = new ParsedMovieInfo
                {
                    Languages = new List<Language> {Language.English}
                },
                Movie = new Movie
                         {
                             Profile = new Profile
                             {
                                 Language = Language.English
                             }
                         }
            };
        }

        private void WithEnglishRelease()
        {
            _remoteMovie.ParsedMovieInfo.Languages = new List<Language> {Language.English};
        }

        private void WithGermanRelease()
        {
            _remoteMovie.ParsedMovieInfo.Languages = new List<Language> {Language.German};
        }

        [Test]
        public void should_return_true_if_language_is_english()
        {
            WithEnglishRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_is_german()
        {
            WithGermanRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_allowed_language_any()
        {
            _remoteMovie.Movie.Profile = new Profile
            {
                Language = Language.Any
            };

            WithGermanRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();

            WithEnglishRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }
    }
}
