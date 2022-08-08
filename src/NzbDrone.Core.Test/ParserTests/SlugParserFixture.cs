using FluentAssertions;

using NUnit.Framework;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class SlugParserFixture : CoreTest
    {
        [TestCase("tèst", "test")]
        [TestCase("têst", "test")]
        [TestCase("tëst", "test")]
        [TestCase("tËst", "test")]
        [TestCase("áccent", "accent")]
        [TestCase("àccent", "accent")]
        [TestCase("âccent", "accent")]
        [TestCase("Äccent", "accent")]
        [TestCase("åccent", "accent")]
        [TestCase("acceñt", "accent")]
        [TestCase("ßtest", "test")]
        [TestCase("œtest", "test")]
        [TestCase("Œtest", "test")]
        [TestCase("Øtest", "test")]
        public void should_replace_accents(string input, string result)
        {
            Parser.Parser.ToUrlSlug(input).Should().Be(result);
        }

        [TestCase("Test'Result")]
        [TestCase("Test$Result")]
        [TestCase("Test(Result")]
        [TestCase("Test)Result")]
        [TestCase("Test*Result")]
        [TestCase("Test?Result")]
        [TestCase("Test/Result")]
        [TestCase("Test=Result")]
        [TestCase("Test\\Result")]
        public void should_replace_special_characters(string input)
        {
            Parser.Parser.ToUrlSlug(input).Should().Be("testresult");
        }

        [TestCase("ThIS IS A MiXeD CaSe SensItIvE ValUe")]
        public void should_lowercase_capitals(string input)
        {
            Parser.Parser.ToUrlSlug(input).Should().Be("this-is-a-mixed-case-sensitive-value");
        }

        [TestCase("test----")]
        [TestCase("test____")]
        [TestCase("test-_--_")]
        public void should_trim_trailing_dashes_and_underscores(string input)
        {
            Parser.Parser.ToUrlSlug(input).Should().Be("test");
        }

        [TestCase("test result")]
        [TestCase("test     result")]
        public void should_replace_spaces_with_dash(string input)
        {
            Parser.Parser.ToUrlSlug(input).Should().Be("test-result");
        }

        [TestCase("test     result", "test-result")]
        [TestCase("test-----result", "test-result")]
        [TestCase("test_____result", "test_result")]
        public void should_replace_double_occurence(string input, string result)
        {
            Parser.Parser.ToUrlSlug(input).Should().Be(result);
        }

        [TestCase("Test'Result")]
        [TestCase("Test$Result")]
        [TestCase("Test(Result")]
        [TestCase("Test)Result")]
        [TestCase("Test*Result")]
        [TestCase("Test?Result")]
        [TestCase("Test/Result")]
        [TestCase("Test=Result")]
        [TestCase("Test\\Result")]
        public void should_replace_special_characters_with_dash_when_enabled(string input)
        {
            Parser.Parser.ToUrlSlug(input, true).Should().Be("test-result");
        }

        [TestCase("Test'Result")]
        [TestCase("Test$Result")]
        [TestCase("Test(Result")]
        [TestCase("Test)Result")]
        [TestCase("Test*Result")]
        [TestCase("Test?Result")]
        [TestCase("Test/Result")]
        [TestCase("Test=Result")]
        [TestCase("Test\\Result")]
        public void should__not_replace_special_characters_with_dash_when_disabled(string input)
        {
            Parser.Parser.ToUrlSlug(input, false).Should().Be("testresult");
        }

        [TestCase("test----", "-_", "test")]
        [TestCase("test____", "-_", "test")]
        [TestCase("test-_-_", "-_", "test")]
        [TestCase("test----", "-", "test")]
        [TestCase("test____", "-", "test____")]
        [TestCase("test-_-_", "-", "test-_-_")]
        [TestCase("test----", "_", "test----")]
        [TestCase("test____", "_", "test")]
        [TestCase("test-_-_", "_", "test-_-")]
        [TestCase("test----", "", "test----")]
        [TestCase("test____", "", "test____")]
        [TestCase("test-_-_", "", "test-_-_")]
        public void should_trim_trailing_dashes_and_underscores_based_on_list(string input, string trimList, string result)
        {
            Parser.Parser.ToUrlSlug(input, false, trimList, "").Should().Be(result);
        }

        [TestCase("test----result", "-_", "test-result")]
        [TestCase("test____result", "-_", "test_result")]
        [TestCase("test_-_-result", "-_", "test-result")]
        [TestCase("test-_-_result", "-_", "test_result")]
        [TestCase("test----result", "-", "test-result")]
        [TestCase("test____result", "-", "test____result")]
        [TestCase("test-_-_result", "-", "test-_-_result")]
        [TestCase("test----result", "_", "test----result")]
        [TestCase("test____result", "_", "test_result")]
        [TestCase("test-_-_result", "_", "test-_-_result")]
        [TestCase("test----result", "", "test----result")]
        [TestCase("test____result", "", "test____result")]
        [TestCase("test-_-_result", "", "test-_-_result")]
        public void should_replace_duplicate_characters_based_on_list(string input, string deduplicateChars, string result)
        {
            Parser.Parser.ToUrlSlug(input, false, "", deduplicateChars).Should().Be(result);
        }

        [Test]
        public void should_handle_null_trim_parameters()
        {
            Parser.Parser.ToUrlSlug("test", false, null, "-_").Should().Be("test");
        }

        [Test]
        public void should_handle_null_dedupe_parameters()
        {
            Parser.Parser.ToUrlSlug("test", false, "-_", null).Should().Be("test");
        }

        [Test]
        public void should_handle_empty_trim_parameters()
        {
            Parser.Parser.ToUrlSlug("test", false, "", "-_").Should().Be("test");
        }

        [Test]
        public void should_handle_empty_dedupe_parameters()
        {
            Parser.Parser.ToUrlSlug("test", false, "-_", "").Should().Be("test");
        }
    }
}
