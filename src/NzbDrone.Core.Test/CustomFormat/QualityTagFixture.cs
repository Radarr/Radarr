using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormat
{
    [TestFixture]
    public class QualityTagFixture : CoreTest
    {
        [TestCase("R_1080", TagType.Resolution, Resolution.R1080P)]
        [TestCase("R_720", TagType.Resolution, Resolution.R720P)]
        [TestCase("R_576", TagType.Resolution, Resolution.R576P)]
        [TestCase("R_480", TagType.Resolution, Resolution.R480P)]
        [TestCase("R_2160", TagType.Resolution, Resolution.R2160P)]
        [TestCase("S_BLURAY", TagType.Source, Source.BLURAY)]
        [TestCase("s_tv", TagType.Source, Source.TV)]
        [TestCase("s_workPRINT", TagType.Source, Source.WORKPRINT)]
        [TestCase("s_Dvd", TagType.Source, Source.DVD)]
        [TestCase("S_WEBdL", TagType.Source, Source.WEBDL)]
        [TestCase("S_CAM", TagType.Source, Source.CAM)]
        [TestCase("L_English", TagType.Language, Language.English)]
        [TestCase("L_germaN", TagType.Language, Language.German)]
        [TestCase("E_Director", TagType.Edition, "director")]
        [TestCase("E_R_Director('?s)?", TagType.Edition, "director('?s)?", TagModifier.Regex)]
        [TestCase("E_RN_Director('?s)?", TagType.Edition, "director('?s)?", TagModifier.Regex, TagModifier.Not)]
        [TestCase("E_RNRE_Director('?s)?", TagType.Edition, "director('?s)?", TagModifier.Regex, TagModifier.Not, TagModifier.AbsolutelyRequired)]
        [TestCase("C_Surround", TagType.Custom, "surround")]
        [TestCase("C_RE_Surround", TagType.Custom, "surround", TagModifier.AbsolutelyRequired)]
        [TestCase("C_REN_Surround", TagType.Custom, "surround", TagModifier.AbsolutelyRequired, TagModifier.Not)]
        [TestCase("C_RENR_Surround|(5|7)(\\.1)?", TagType.Custom, "surround|(5|7)(\\.1)?", TagModifier.AbsolutelyRequired, TagModifier.Not, TagModifier.Regex)]
        [TestCase("G_10<>20", TagType.Size, new[] { 10.0, 20.0})]
        [TestCase("G_15.55<>20", TagType.Size, new[] { 15.55, 20.0})]
        [TestCase("G_15.55<>25.1908754", TagType.Size, new[] { 15.55, 25.1908754})]
        public void should_parse_tag_from_string(string raw, TagType type, object value, params TagModifier[] modifiers)
        {
            var parsed = new FormatTag(raw);
            TagModifier modifier = 0;
            foreach (var m in modifiers)
            {
                modifier |= m;
            }
            parsed.TagType.Should().Be(type);
            if (value is double[])
            {
                value = (((double[]) value)[0], ((double[]) value)[1]);
            }
            if ((parsed.Value as Regex) != null)
            {
                (parsed.Value as Regex).ToString().Should().Be((value as string));
            }
            else
            {
                parsed.Value.Should().Be(value);
            }
            parsed.TagModifier.Should().Be(modifier);
        }
    }
}
