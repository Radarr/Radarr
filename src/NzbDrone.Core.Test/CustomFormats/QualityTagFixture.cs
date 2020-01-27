using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormats
{
    [TestFixture]
    public class QualityTagFixture : CoreTest
    {
        [TestCase("R_1080", TagType.Resolution, Resolution.R1080p)]
        [TestCase("R_720", TagType.Resolution, Resolution.R720p)]
        [TestCase("R_576", TagType.Resolution, Resolution.R576p)]
        [TestCase("R_480", TagType.Resolution, Resolution.R480p)]
        [TestCase("R_2160", TagType.Resolution, Resolution.R2160p)]
        [TestCase("S_BLURAY", TagType.Source, Source.BLURAY)]
        [TestCase("s_tv", TagType.Source, Source.TV)]
        [TestCase("s_workPRINT", TagType.Source, Source.WORKPRINT)]
        [TestCase("s_Dvd", TagType.Source, Source.DVD)]
        [TestCase("S_WEBdL", TagType.Source, Source.WEBDL)]
        [TestCase("S_CAM", TagType.Source, Source.CAM)]

        // [TestCase("L_English", TagType.Language, Language.English)]
        // [TestCase("L_Italian", TagType.Language, Language.Italian)]
        //  [TestCase("L_iTa", TagType.Language, Language.Italian)]
        // [TestCase("L_germaN", TagType.Language, Language.German)]
        [TestCase("E_Director", TagType.Edition, "director")]
        [TestCase("E_RX_Director('?s)?", TagType.Edition, "director('?s)?", TagModifier.Regex)]
        [TestCase("E_RXN_Director('?s)?", TagType.Edition, "director('?s)?", TagModifier.Regex, TagModifier.Not)]
        [TestCase("E_RXNRQ_Director('?s)?", TagType.Edition, "director('?s)?", TagModifier.Regex, TagModifier.Not, TagModifier.AbsolutelyRequired)]
        [TestCase("C_Surround", TagType.Custom, "surround")]
        [TestCase("C_RQ_Surround", TagType.Custom, "surround", TagModifier.AbsolutelyRequired)]
        [TestCase("C_RQN_Surround", TagType.Custom, "surround", TagModifier.AbsolutelyRequired, TagModifier.Not)]
        [TestCase("C_RQNRX_Surround|(5|7)(\\.1)?", TagType.Custom, "surround|(5|7)(\\.1)?", TagModifier.AbsolutelyRequired, TagModifier.Not, TagModifier.Regex)]
        [TestCase("G_10<>20", TagType.Size, new[] { 10737418240L, 21474836480L })]
        [TestCase("G_15.55<>20", TagType.Size, new[] { 16696685363L, 21474836480L })]
        [TestCase("G_15.55<>25.1908754", TagType.Size, new[] { 16696685363L, 27048496500L })]
        [TestCase("R__1080", TagType.Resolution, Resolution.R1080p)]
        public void should_parse_tag_from_string(string raw, TagType type, object value, params TagModifier[] modifiers)
        {
            var parsed = new FormatTag(raw);
            TagModifier modifier = 0;
            foreach (var m in modifiers)
            {
                modifier |= m;
            }

            parsed.TagType.Should().Be(type);
            if (value is long[])
            {
                value = (((long[])value)[0], ((long[])value)[1]);
            }

            if ((parsed.Value as Regex) != null)
            {
                (parsed.Value as Regex).ToString().Should().Be(value as string);
            }
            else
            {
                parsed.Value.Should().Be(value);
            }

            parsed.TagModifier.Should().Be(modifier);
        }
    }
}
