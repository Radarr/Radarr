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
    public class fix_format_tags_double_underscoreFixture : MigrationTest<fix_format_tags_double_underscore>
    {
        public void AddCustomFormat(fix_format_tags_double_underscore c, string name, params string[] formatTags)
        {
            var customFormat = new { Name = name, FormatTags = formatTags.ToList().ToJson() };

            c.Insert.IntoTable("CustomFormats").Row(customFormat);
        }

        [TestCase("C_HDR", "C_HDR")]
        [TestCase("C__HDR", "C_HDR")]
        [TestCase("C_RXRQ_HDR", "C_RXRQ_HDR")]
        [TestCase("C_RENR_HDR", "C_RENR_HDR")]
        [TestCase("E__Director", "E_Director")]
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

        private List<regex_required_tagsFixture.CustomFormatTest149> QueryItems(IDirectDataMapper db)
        {
            var items = db.Query<regex_required_tagsFixture.CustomFormatTest149>("SELECT \"Name\", \"FormatTags\" FROM \"CustomFormats\"");

            return items.Select(i =>
            {
                i.DeserializedTags = JsonConvert.DeserializeObject<List<string>>(i.FormatTags);
                return i;
            }).ToList();
        }
    }
}
