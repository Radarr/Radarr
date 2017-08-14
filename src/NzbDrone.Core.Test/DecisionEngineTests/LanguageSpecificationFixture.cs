using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class LanguageSpecificationFixture : CoreTest
    {
        private RemoteAlbum _remoteAlbum;

        [SetUp]
        public void Setup()
        {
            _remoteAlbum = new RemoteAlbum
            {
                ParsedAlbumInfo = new ParsedAlbumInfo
                {
                    Language = Language.English
                },
                Artist = new Artist
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
            _remoteAlbum.ParsedAlbumInfo.Language = Language.English;
        }

        private void WithGermanRelease()
        {
            _remoteAlbum.ParsedAlbumInfo.Language = Language.German;            
        }

        [Test]
        public void should_return_true_if_language_is_english()
        {
            WithEnglishRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_is_german()
        {
            WithGermanRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }
    }
}