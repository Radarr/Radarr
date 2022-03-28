using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class regex_required_tagsFixture : MigrationTest<convert_regex_required_tags>
    {
        public void AddCustomFormat(convert_regex_required_tags c, string name, params string[] formatTags)
        {
            var customFormat = new { Name = name, FormatTags = formatTags.ToList().ToJson() };

            c.Insert.IntoTable("CustomFormats").Row(customFormat);
        }

        [TestCase("C_RE_HDR", "C_RQ_HDR")]
        [TestCase("C_R_HDR", "C_RX_HDR")]
        [TestCase("C_RER_HDR", "C_RXRQ_HDR")]
        [TestCase("C_RENR_HDR", "C_NRXRQ_HDR")]
        [TestCase("C_NRER_HDR", "C_NRXRQ_HDR")]
        [TestCase("C_RE_RERN", "C_RQ_RERN")]
        [TestCase("E_NRER_Director", "E_NRXRQ_Director")]
        [TestCase("G_N_1000<>1000", "G_N_1000<>1000")]
        [TestCase("G_1000<>1000", "G_1000<>1000")]
        public void should_correctly_convert_format_tag(string original, string converted)
        {
            var db = WithMigrationTestDb(c => { AddCustomFormat(c, "TestFormat", original); });

            var items = QueryItems(db);

            var convertedTags = items.First().DeserializedTags;

            convertedTags.Should().HaveCount(1);
            convertedTags.First().Should().BeEquivalentTo(converted);
        }

        [Test]
        public void should_correctly_convert_multiple()
        {
            var db = WithMigrationTestDb(c => { AddCustomFormat(c, "TestFormat", "C_RE_HDR", "C_R_HDR", "E_NRER_Director"); });

            var items = QueryItems(db);

            var convertedTags = items.First().DeserializedTags;

            convertedTags.Should().HaveCount(3);
            convertedTags.Should().BeEquivalentTo("C_RQ_HDR", "C_RX_HDR", "E_NRXRQ_Director");
        }

        [Test]
        public void should_correctly_convert_multiple_formats()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddCustomFormat(c, "TestFormat", "C_RE_HDR", "C_R_HDR", "E_NRER_Director");
                AddCustomFormat(c, "TestFormat2", "E_NRER_Director");
            });

            var items = QueryItems(db);

            var convertedTags = items.First().DeserializedTags;

            convertedTags.Should().HaveCount(3);
            convertedTags.Should().BeEquivalentTo("C_RQ_HDR", "C_RX_HDR", "E_NRXRQ_Director");

            var convertedTags2 = items.Last().DeserializedTags;

            convertedTags2.Should().HaveCount(1);
            convertedTags2.Should().BeEquivalentTo("E_NRXRQ_Director");
        }

        private List<CustomFormatTest149> QueryItems(IDirectDataMapper db)
        {
            var items = db.Query<CustomFormatTest149>("SELECT \"Name\", \"FormatTags\" FROM \"CustomFormats\"");

            return items.Select(i =>
            {
                i.DeserializedTags = JsonConvert.DeserializeObject<List<string>>(i.FormatTags);
                return i;
            }).ToList();
        }

        public class CustomFormatTest149
        {
            public string Name { get; set; }
            public string FormatTags { get; set; }
            public List<string> DeserializedTags { get; set; }
        }
    }
}
