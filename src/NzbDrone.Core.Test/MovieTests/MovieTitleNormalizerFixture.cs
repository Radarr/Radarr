using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Test.MovieTests
{
    [TestFixture]
    public class MovieTitleNormalizerFixture
    {
        //TODO: Decide on reimplementing this!
        /*
        [TestCase("A to Z", 281588, "a to z")]
        [TestCase("A. D. - The Trials & Triumph of the Early Church", 266757, "ad trials triumph early church")]
        public void should_use_precomputed_title(string title, int tvdbId, string expected)
        {
            MovieTitleNormalizer.Normalize(title, tvdbId).Should().Be(expected);
        }*/

        [TestCase("2 Broke Girls", "2 broke girls")]
        [TestCase("Archer (2009)", "archer 2009")]
        [TestCase("The Office (US)", "office us")]
        [TestCase("The Mentalist", "mentalist")]
        [TestCase("The Good Wife", "good wife")]
        [TestCase("The Newsroom (2012)", "newsroom 2012")]
        [TestCase("Special Agent Oso", "special agent oso")]
        public void should_normalize_title(string title, string expected)
        {
            MovieTitleNormalizer.Normalize(title, 0).Should().Be(expected);
        }
    }
}
