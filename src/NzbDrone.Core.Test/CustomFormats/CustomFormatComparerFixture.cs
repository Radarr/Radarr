using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class CustomFormatsComparerFixture : CoreTest
    {
        private CustomFormat _customFormat1;
        private CustomFormat _customFormat2;
        private CustomFormat _customFormat3;
        private CustomFormat _customFormat4;

        public CustomFormatsComparer Subject { get; set; }

        [SetUp]
        public void Setup()
        {
        }

        private void GivenDefaultProfileWithFormats()
        {
            _customFormat1 = new CustomFormat("My Format 1", new LanguageSpecification { Value = (int)Language.English }) { Id = 1 };
            _customFormat2 = new CustomFormat("My Format 2", new LanguageSpecification { Value = (int)Language.French }) { Id = 2 };
            _customFormat3 = new CustomFormat("My Format 3", new LanguageSpecification { Value = (int)Language.Spanish }) { Id = 3 };
            _customFormat4 = new CustomFormat("My Format 4", new LanguageSpecification { Value = (int)Language.Italian }) { Id = 4 };

            CustomFormatsFixture.GivenCustomFormats(CustomFormat.None, _customFormat1, _customFormat2, _customFormat3, _customFormat4);

            Subject = new CustomFormatsComparer(new Profile { Items = QualityFixture.GetDefaultQualities(), FormatItems = CustomFormatsFixture.GetSampleFormatItems() });
        }

        [Test]
        public void should_be_lesser_when_first_format_is_worse()
        {
            GivenDefaultProfileWithFormats();

            var first = new List<CustomFormat> { _customFormat1 };
            var second = new List<CustomFormat> { _customFormat2 };

            var compare = Subject.Compare(first, second);

            compare.Should().BeLessThan(0);
        }

        [Test]
        public void should_be_zero_when_formats_are_equal()
        {
            GivenDefaultProfileWithFormats();

            var first = new List<CustomFormat> { _customFormat2 };
            var second = new List<CustomFormat> { _customFormat2 };

            var compare = Subject.Compare(first, second);

            compare.Should().Be(0);
        }

        [Test]
        public void should_be_greater_when_first_format_is_better()
        {
            GivenDefaultProfileWithFormats();

            var first = new List<CustomFormat> { _customFormat3 };
            var second = new List<CustomFormat> { _customFormat2 };

            var compare = Subject.Compare(first, second);

            compare.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_greater_when_multiple_formats_better()
        {
            GivenDefaultProfileWithFormats();

            var first = new List<CustomFormat> { _customFormat3, _customFormat4 };
            var second = new List<CustomFormat> { _customFormat2 };

            var compare = Subject.Compare(first, second);

            compare.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_greater_when_best_format_is_better()
        {
            GivenDefaultProfileWithFormats();

            var first = new List<CustomFormat> { _customFormat1, _customFormat3 };
            var second = new List<CustomFormat> { _customFormat2 };

            var compare = Subject.Compare(first, second);

            compare.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_greater_when_best_format_equal_but_more_lower_formats()
        {
            GivenDefaultProfileWithFormats();

            var first = new List<CustomFormat> { _customFormat1, _customFormat2 };
            var second = new List<CustomFormat> { _customFormat2 };

            var compare = Subject.Compare(first, second);

            compare.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_not_be_greater_when_best_format_worse_but_more_lower_formats()
        {
            GivenDefaultProfileWithFormats();

            var first = new List<CustomFormat> { _customFormat1, _customFormat2, _customFormat3 };
            var second = new List<CustomFormat> { _customFormat4 };

            var compare = Subject.Compare(first, second);

            compare.Should().BeLessThan(0);
        }
    }
}
