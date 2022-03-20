using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;

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
                    Languages = new List<Language> { Language.English }
                },
                Movie = new Movie
                         {
                             Profile = new Profile
                             {
                                 Language = Language.English
                             },
                             MovieMetadata = new MovieMetadata
                             {
                                 OriginalLanguage = Language.French
                             }
                         }
            };
        }

        private void WithEnglishRelease()
        {
            _remoteMovie.ParsedMovieInfo.Languages = new List<Language> { Language.English };
        }

        private void WithGermanRelease()
        {
            _remoteMovie.ParsedMovieInfo.Languages = new List<Language> { Language.German };
        }

        private void WithFrenchRelease()
        {
            _remoteMovie.ParsedMovieInfo.Languages = new List<Language> { Language.French };
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
        public void should_return_false_if_release_is_german_and_profile_original()
        {
            _remoteMovie.Movie.Profile.Language = Language.Original;

            WithGermanRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_release_is_french_and_profile_original()
        {
            _remoteMovie.Movie.Profile.Language = Language.Original;

            WithFrenchRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
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
