using System.Collections.Generic;
using System.Linq;
using Dapper;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class custom_format_rework_parserFixture : CoreTest<custom_format_rework>
    {
        [TestCase(@"C_RX_(x|h)\.?264", "ReleaseTitleSpecification", false, false, @"(x|h)\.?264")]
        [TestCase(@"C_(hello)", "ReleaseTitleSpecification", false, false, @"\(hello\)")]
        [TestCase("C_Surround", "ReleaseTitleSpecification", false, false, "surround")]
        [TestCase("C_RQ_Surround", "ReleaseTitleSpecification", true, false, "surround")]
        [TestCase("C_RQN_Surround", "ReleaseTitleSpecification", true, true, "surround")]
        [TestCase("C_RQNRX_Surround|(5|7)(\\.1)?", "ReleaseTitleSpecification", true, true, "surround|(5|7)(\\.1)?")]
        [TestCase("R_1080", "ResolutionSpecification", false, false, (int)Resolution.R1080p)]
        [TestCase("R__1080", "ResolutionSpecification", false, false, (int)Resolution.R1080p)]
        [TestCase("R_720", "ResolutionSpecification", false, false, (int)Resolution.R720p)]
        [TestCase("R_576", "ResolutionSpecification", false, false, (int)Resolution.R576p)]
        [TestCase("R_480", "ResolutionSpecification", false, false, (int)Resolution.R480p)]
        [TestCase("R_2160", "ResolutionSpecification", false, false, (int)Resolution.R2160p)]
        [TestCase("S_BLURAY", "SourceSpecification", false, false, (int)Source.BLURAY)]
        [TestCase("s_tv", "SourceSpecification", false, false, (int)Source.TV)]
        [TestCase("s_workPRINT", "SourceSpecification", false, false, (int)Source.WORKPRINT)]
        [TestCase("s_Dvd", "SourceSpecification", false, false, (int)Source.DVD)]
        [TestCase("S_WEBdL", "SourceSpecification", false, false, (int)Source.WEBDL)]
        [TestCase("S_CAM", "SourceSpecification", false, false, (int)Source.CAM)]
        [TestCase("L_English", "LanguageSpecification", false, false, 1)]
        [TestCase("L_Italian", "LanguageSpecification", false, false, 5)]
        [TestCase("L_iTa", "LanguageSpecification", false, false, 5)]
        [TestCase("L_germaN", "LanguageSpecification", false, false, 4)]
        [TestCase("E_Director", "EditionSpecification", false, false, "director")]
        [TestCase("E_RX_Director('?s)?", "EditionSpecification", false, false, "director(\u0027?s)?")]
        [TestCase("E_RXN_Director('?s)?", "EditionSpecification", false, true, "director(\u0027?s)?")]
        [TestCase("E_RXNRQ_Director('?s)?", "EditionSpecification", true, true, "director(\u0027?s)?")]
        public void should_convert_custom_format(string raw, string specType, bool required, bool negated, object value)
        {
            var format = Subject.ParseFormatTag(raw);
            format.Negate.Should().Be(negated);
            format.Required.Should().Be(required);

            format.ToJson().Should().Contain(JsonConvert.ToString(value));
        }

        [TestCase("G_10<>20", "SizeSpecification", 10, 20)]
        [TestCase("G_15.55<>20", "SizeSpecification", 15.55, 20)]
        [TestCase("G_15.55<>25.1908754", "SizeSpecification", 15.55, 25.1908754)]
        public void should_convert_size_cf(string raw, string specType, double min, double max)
        {
            var format = Subject.ParseFormatTag(raw) as SizeSpecification;
            format.Negate.Should().Be(false);
            format.Required.Should().Be(false);
            format.Min.Should().Be(min);
            format.Max.Should().Be(max);
        }
    }

    [TestFixture]
    public class custom_format_reworkFixture : MigrationTest<custom_format_rework>
    {
        [Test]
        public void should_convert_custom_format_row_with_one_spec()
        {
            var db = WithDapperMigrationTestDb(c =>
                {
                    c.Insert.IntoTable("CustomFormats").Row(new
                    {
                        Id = 1,
                        Name = "Test",
                        FormatTags = new List<string> { @"C_(hello)" }.ToJson()
                    });
                });

            var json = db.Query<string>("SELECT \"Specifications\" FROM \"CustomFormats\"").First();

            ValidateFormatTag(json, "ReleaseTitleSpecification", false, false);
            json.Should().Contain($"\"name\": \"Test\"");
        }

        [Test]
        public void should_convert_custom_format_row_with_two_specs()
        {
            var db = WithDapperMigrationTestDb(c =>
                {
                    c.Insert.IntoTable("CustomFormats").Row(new
                    {
                        Id = 1,
                        Name = "Test",
                        FormatTags = new List<string> { @"C_(hello)", "E_Director" }.ToJson()
                    });
                });

            var json = db.Query<string>("SELECT \"Specifications\" FROM \"CustomFormats\"").First();

            ValidateFormatTag(json, "ReleaseTitleSpecification", false, false);
            ValidateFormatTag(json, "EditionSpecification", false, false);
            json.Should().Contain($"\"name\": \"Release Title 1\"");
            json.Should().Contain($"\"name\": \"Edition 1\"");
        }

        private void ValidateFormatTag(string json, string type, bool required, bool negated)
        {
            json.Should().Contain($"\"type\": \"{type}\"");

            if (required)
            {
                json.Should().Contain($"\"required\": true");
            }

            if (negated)
            {
                json.Should().Contain($"\"negate\": true");
            }
        }
    }
}
