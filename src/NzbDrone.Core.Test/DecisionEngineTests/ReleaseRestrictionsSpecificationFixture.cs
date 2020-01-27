using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Restrictions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class ReleaseRestrictionsSpecificationFixture : CoreTest<ReleaseRestrictionsSpecification>
    {
        private RemoteMovie _remoteMovie;

        [SetUp]
        public void Setup()
        {
            _remoteMovie = new RemoteMovie
            {
                Movie = new Movie
                {
                    Tags = new HashSet<int>()
                },
                Release = new ReleaseInfo
                {
                    Title = "Dexter.S08E01.EDITED.WEBRip.x264-KYR"
                }
            };

            Mocker.SetConstant<ITermMatcher>(Mocker.Resolve<TermMatcher>());
        }

        private void GivenRestictions(string required, string ignored)
        {
            Mocker.GetMock<IRestrictionService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<int>>()))
                  .Returns(new List<Restriction>
                           {
                               new Restriction
                               {
                                   Required = required,
                                   Ignored = ignored
                               }
                           });
        }

        [Test]
        public void should_be_true_when_restrictions_are_empty()
        {
            Mocker.GetMock<IRestrictionService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<int>>()))
                  .Returns(new List<Restriction>());

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_title_contains_one_required_term()
        {
            GivenRestictions("WEBRip", null);

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_title_does_not_contain_any_required_terms()
        {
            GivenRestictions("doesnt,exist", null);

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_when_title_does_not_contain_any_ignored_terms()
        {
            GivenRestictions(null, "ignored");

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_title_contains_one_anded_ignored_terms()
        {
            GivenRestictions(null, "edited");

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [TestCase("EdiTED")]
        [TestCase("webrip")]
        [TestCase("X264")]
        [TestCase("X264,NOTTHERE")]
        public void should_ignore_case_when_matching_required(string required)
        {
            GivenRestictions(required, null);

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [TestCase("EdiTED")]
        [TestCase("webrip")]
        [TestCase("X264")]
        [TestCase("X264,NOTTHERE")]
        public void should_ignore_case_when_matching_ignored(string ignored)
        {
            GivenRestictions(null, ignored);

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_false_when_release_contains_one_restricted_word_and_one_required_word()
        {
            _remoteMovie.Release.Title = "[ www.Speed.cd ] -Whose.Line.is.it.Anyway.US.S10E24.720p.HDTV.x264-BAJSKORV";

            Mocker.GetMock<IRestrictionService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<int>>()))
                  .Returns(new List<Restriction>
                           {
                               new Restriction { Required = "x264", Ignored = "www.Speed.cd" }
                           });

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [TestCase("/WEB/", true)]
        [TestCase("/WEB\b/", false)]
        [TestCase("/WEb/", false)]
        [TestCase(@"/\.WEB/", true)]
        public void should_match_perl_regex(string pattern, bool expected)
        {
            GivenRestictions(pattern, null);

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().Be(expected);
        }
    }
}
