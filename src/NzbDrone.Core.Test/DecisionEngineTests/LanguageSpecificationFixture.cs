using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class LanguageSpecificationFixture : CoreTest
    {
        private RemoteMovie _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode = new RemoteMovie
            {
                ParsedMovieInfo = new ParsedMovieInfo
                {
                    Language = Language.English
                },
                Movie = new Movie
                         {
                             Profile = new LazyLoaded<Profile>(new Profile
                                                               {
                                                                   Language = Language.English
                                                               })
                         }
            };
        }

        private void WithEnglishRelease()
        {
            _remoteEpisode.ParsedMovieInfo.Language = Language.English;
        }

        private void WithGermanRelease()
        {
            _remoteEpisode.ParsedMovieInfo.Language = Language.German;            
        }

        [Test]
        public void should_return_true_if_language_is_english()
        {
            WithEnglishRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_is_german()
        {
            WithGermanRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }
    }
}
