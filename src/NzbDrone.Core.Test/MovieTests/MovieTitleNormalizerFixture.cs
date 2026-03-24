using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Test.MovieTests
{
    [TestFixture]
    public class MovieTitleNormalizerFixture
    {
        [TestCase("A to Z", 387354, "a to z")]
        [TestCase("A to Z", 1212922, "a to z")]
        [TestCase("A to Z: The First Alphabet", 888700, "a to z the first alphabet")]
        [TestCase("A to Zeppelin: The Story of Led Zeppelin", 101273, "a to zeppelin the story of led zeppelin")]
        public void should_use_precomputed_title(string title, int tmdbId, string expected)
        {
            MovieTitleNormalizer.Normalize(title, tmdbId).Should().Be(expected);
        }

        [TestCase("2 Broke Girls", "2 broke girls")]
        [TestCase("Archer (2009)", "archer 2009")]
        [TestCase("The Office (US)", "office us")]
        [TestCase("The Mentalist", "mentalist")]
        [TestCase("The Good Wife", "good wife")]
        [TestCase("The Newsroom (2012)", "newsroom 2012")]
        [TestCase("Special Agent Oso", "special agent oso")]
        [TestCase("A.N.T. Farm", "ant farm")]
        [TestCase("A.I.C.O. -Incarnation-", "aico incarnation")]
        [TestCase("A.D. The Bible Continues", "ad the bible continues")]
        [TestCase("A.P. Bio", "ap bio")]
        [TestCase("A-Team", "ateam")]
        [TestCase("The A-Team", "ateam")]
        [TestCase("And Just Like That", "and just like that")]
        [TestCase("A.I. Artificial Intelligence", "ai artificial intelligence")]
        [TestCase("An A to Z of English", "a to z of english")]
        public void should_normalize_title(string title, string expected)
        {
            MovieTitleNormalizer.Normalize(title, 0).Should().Be(expected);
        }
    }
}
