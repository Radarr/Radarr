using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.Profiles.Releases.PreferredWordService
{
    [TestFixture]
    public class CalculateFixture : CoreTest<Core.Profiles.Releases.PreferredWordService>
    {
        private Artist _artist = null;
        private List<ReleaseProfile> _releaseProfiles = null;
        private string _title = "Artist.Name-Album.Title.2018.FLAC.24bit-Lidarr";

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateNew()
                                     .With(s => s.Tags = new HashSet<int>(new[] {1, 2}))
                                     .Build();

            _releaseProfiles = new List<ReleaseProfile>();

            _releaseProfiles.Add(new ReleaseProfile
                                 {
                                     Preferred = new List<KeyValuePair<string, int>>
                                                 {
                                                     new KeyValuePair<string, int>("24bit", 5),
                                                     new KeyValuePair<string, int>("16bit", -10)
                                                 }
                                 });

            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<int>>()))
                  .Returns(_releaseProfiles);
        }

            
        private void GivenMatchingTerms(params string[] terms)
        {
            Mocker.GetMock<ITermMatcherService>()
                  .Setup(s => s.IsMatch(It.IsAny<string>(), _title))
                  .Returns<string, string>((term, title) => terms.Contains(term));
        }

        [Test]
        public void should_return_0_when_there_are_no_release_profiles()
        {
            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<int>>()))
                  .Returns(new List<ReleaseProfile>());

            Subject.Calculate(_artist, _title).Should().Be(0);
        }

        [Test]
        public void should_return_0_when_there_are_no_matching_preferred_words()
        {
            GivenMatchingTerms();

            Subject.Calculate(_artist, _title).Should().Be(0);
        }

        [Test]
        public void should_calculate_positive_score()
        {
            GivenMatchingTerms("24bit");

            Subject.Calculate(_artist, _title).Should().Be(5);
        }

        [Test]
        public void should_calculate_negative_score()
        {
            GivenMatchingTerms("16bit");

            Subject.Calculate(_artist, _title).Should().Be(-10);
        }

        [Test]
        public void should_calculate_using_multiple_profiles()
        {
            _releaseProfiles.Add(_releaseProfiles.First());

            GivenMatchingTerms("24bit");

            Subject.Calculate(_artist, _title).Should().Be(10);
        }
    }
}
