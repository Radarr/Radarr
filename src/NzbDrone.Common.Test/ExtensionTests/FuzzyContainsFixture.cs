using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class FuzzyContainsFixture : TestBase
    {
        [TestCase("abcdef", "abcdef", 0.5, 0)]
        [TestCase("", "abcdef", 0.5, -1)]
        [TestCase("abcdef", "", 0.5, -1)]
        [TestCase("", "", 0.5, -1)]
        [TestCase("abcdef", "de", 0.5, 3)]
        [TestCase("abcdef", "defy", 0.5, 3)]
        [TestCase("abcdef", "abcdefy", 0.5, 0)]
        [TestCase("I am the very model of a modern major general.", " that berry ", 0.3, 4)]
        [TestCase("abcdefghijk", "fgh", 0.5, 5)]
        [TestCase("abcdefghijk", "fgh", 0.5, 5)]
        [TestCase("abcdefghijk", "efxhi", 0.5, 4)]
        [TestCase("abcdefghijk", "cdefxyhijk", 0.5, 2)]
        [TestCase("abcdefghijk", "bxy", 0.5, -1)]
        [TestCase("123456789xx0", "3456789x0", 0.5, 2)]
        [TestCase("abcdef", "xxabc", 0.5, 0)]
        [TestCase("abcdef", "defyy", 0.5, 3)]
        [TestCase("abcdef", "xabcdefy", 0.5, 0)]
        [TestCase("abcdefghijk", "efxyhi", 0.6, 4)]
        [TestCase("abcdefghijk", "efxyhi", 0.7, -1)]
        [TestCase("abcdefghijk", "bcdef", 0.0, 1)]
        [TestCase("abcdexyzabcde", "abccde", 0.5, 0)]
        [TestCase("abcdefghijklmnopqrstuvwxyz", "abcdxxefg", 0.5, 0)]
        [TestCase("abcdefghijklmnopqrstuvwxyz", "abcdefg", 0.5, 0)]
        [TestCase("The quick brown fox jumps over the lazy dog", "The quick brown fox jumps over the lazy d", 0.5, 0)]
        [TestCase("The quick brown fox jumps over the lazy dog", "The quick brown fox jumps over the lazy g", 0.5, 0)]
        [TestCase("The quick brown fox jumps over the lazy dog", "quikc brown fox jumps over the lazy dog", 0.5, 4)]
        [TestCase("The quick brown fox jumps over the lazy dog", "qui jumps over the lazy dog", 0.5, 16)]
        [TestCase("The quick brown fox jumps over the lazy dog", "quikc brown fox jumps over the lazy dog", 0.5, 4)]
        [TestCase("u6IEytQiYpzAccsbjQ5ISuE4smDQ1ZiU42cFBrTeKB2XrVLEqAvgIiKlDP75iApy07jzmK", "xEytQiYpzAccsbjQ5ISuE4smDQ1ZiU42cFBrTeKB2XrVLEqAvgIiKlDP75iApy07jzmK", 0.5, 2)]
        [TestCase("plusifeelneedforredundantinformationintitlefield", "anthology", 0.5, -1)]
        public void FuzzyFind(string text, string pattern, double threshold, int expected)
        {
            text.FuzzyFind(pattern, threshold).Should().Be(expected);
        }

        [TestCase("abcdef", "abcdef", 1)]
        [TestCase("", "abcdef", 0)]
        [TestCase("abcdef", "", 0)]
        [TestCase("", "", 0)]
        [TestCase("abcdef", "de", 1)]
        [TestCase("abcdef", "defy", 0.75)]
        [TestCase("abcdef", "abcdefghk", 6.0/9)]
        [TestCase("abcdef", "zabcdefz", 6.0/8)]
        [TestCase("plusifeelneedforredundantinformationintitlefield", "anthology", 4.0/9)]
        [TestCase("+ (Plus) - I feel the need for redundant information in the title field", "+", 1)]
        public void FuzzyContains(string text, string pattern, double expectedScore)
        {
            text.FuzzyContains(pattern).Should().BeApproximately(expectedScore, 1e-9);
        }
    }
}
