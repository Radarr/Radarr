using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityFixture : CoreTest
    {
        public static object[] FromIntCases =
                {
                        new object[] { 0, Quality.Unknown },
                        new object[] { 1, Quality.PDF },
                        new object[] { 2, Quality.MOBI },
                        new object[] { 3, Quality.EPUB },
                        new object[] { 4, Quality.AZW3 },
                        new object[] { 10, Quality.MP3_320 },
                        new object[] { 11, Quality.FLAC },
                };

        public static object[] ToIntCases =
                {
                        new object[] { Quality.Unknown, 0 },
                        new object[] { Quality.PDF, 1 },
                        new object[] { Quality.MOBI, 2 },
                        new object[] { Quality.EPUB, 3 },
                        new object[] { Quality.AZW3, 4 },
                        new object[] { Quality.MP3_320, 10 },
                        new object[] { Quality.FLAC, 11 },
                };

        [Test]
        [TestCaseSource(nameof(FromIntCases))]
        public void should_be_able_to_convert_int_to_qualityTypes(int source, Quality expected)
        {
            var quality = (Quality)source;
            quality.Should().Be(expected);
        }

        [Test]
        [TestCaseSource(nameof(ToIntCases))]
        public void should_be_able_to_convert_qualityTypes_to_int(Quality source, int expected)
        {
            var i = (int)source;
            i.Should().Be(expected);
        }

        public static List<QualityProfileQualityItem> GetDefaultQualities(params Quality[] allowed)
        {
            var qualities = new List<Quality>
            {
                Quality.Unknown,
                Quality.MOBI,
                Quality.EPUB,
                Quality.AZW3,
                Quality.MP3_320,
                Quality.FLAC
            };

            if (allowed.Length == 0)
            {
                allowed = qualities.ToArray();
            }

            var items = qualities
                .Except(allowed)
                .Concat(allowed)
                .Select(v => new QualityProfileQualityItem { Quality = v, Allowed = allowed.Contains(v) }).ToList();

            return items;
        }
    }
}
