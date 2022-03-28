using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class custom_formatsFixture : MigrationTest<add_custom_formats>
    {
        public static Dictionary<int, int> QualityToDefinition = null;

        public void AddDefaultProfile(add_custom_formats m, string name, Quality cutoff, params Quality[] allowed)
        {
            var items = Quality.DefaultQualityDefinitions
                .OrderBy(v => v.Weight)
                .Select(v => new { Quality = (int)v.Quality, Allowed = allowed.Contains(v.Quality) })
                .ToList();

            var profile = new { Name = name, Cutoff = (int)cutoff, Items = items.ToJson(), Language = (int)Language.English };

            m.Insert.IntoTable("Profiles").Row(profile);
        }

        public void WithDefaultProfiles(add_custom_formats m)
        {
            AddDefaultProfile(m,
                "Any",
                Quality.Bluray480p,
                Quality.WORKPRINT,
                Quality.CAM,
                Quality.TELESYNC,
                Quality.TELECINE,
                Quality.DVDSCR,
                Quality.REGIONAL,
                Quality.SDTV,
                Quality.DVD,
                Quality.DVDR,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.HDTV2160p,
                Quality.WEBDL480p,
                Quality.WEBDL720p,
                Quality.WEBDL1080p,
                Quality.WEBDL2160p,
                Quality.Bluray480p,
                Quality.Bluray576p,
                Quality.Bluray720p,
                Quality.Bluray1080p,
                Quality.Bluray2160p,
                Quality.Remux1080p,
                Quality.Remux2160p,
                Quality.BRDISK);

            AddDefaultProfile(m,
                "SD",
                Quality.Bluray480p,
                Quality.WORKPRINT,
                Quality.CAM,
                Quality.TELESYNC,
                Quality.TELECINE,
                Quality.DVDSCR,
                Quality.REGIONAL,
                Quality.SDTV,
                Quality.DVD,
                Quality.WEBDL480p,
                Quality.Bluray480p,
                Quality.Bluray576p);

            AddDefaultProfile(m,
                "HD-720p",
                Quality.Bluray720p,
                Quality.HDTV720p,
                Quality.WEBDL720p,
                Quality.Bluray720p);

            AddDefaultProfile(m,
                "HD-1080p",
                Quality.Bluray1080p,
                Quality.HDTV1080p,
                Quality.WEBDL1080p,
                Quality.Bluray1080p,
                Quality.Remux1080p);

            AddDefaultProfile(m,
                "Ultra-HD",
                Quality.Remux2160p,
                Quality.HDTV2160p,
                Quality.WEBDL2160p,
                Quality.Bluray2160p,
                Quality.Remux2160p);

            AddDefaultProfile(m,
                "HD - 720p/1080p",
                Quality.Bluray720p,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.WEBDL720p,
                Quality.WEBDL1080p,
                Quality.Bluray720p,
                Quality.Bluray1080p,
                Quality.Remux1080p,
                Quality.Remux2160p);
        }

        [Test]
        public void should_correctly_update_items_of_default_profiles()
        {
            var db = WithMigrationTestDb(c =>
            {
                WithDefaultProfiles(c);
            });

            ShouldHaveAddedDefaultFormat(db);
        }

        private void ShouldHaveAddedDefaultFormat(IDirectDataMapper db)
        {
            var items = QueryItems(db);

            foreach (var item in items)
            {
                item.DeserializedItems.Count.Should().Be(1);
                item.DeserializedItems.First().Allowed.Should().Be(true);
                item.FormatCutoff.Should().Be(0);
            }
        }

        private List<Profile147> QueryItems(IDirectDataMapper db)
        {
            var test = db.Query("SELECT * FROM \"Profiles\"");

            var items = db.Query<Profile147>("SELECT \"FormatItems\", \"FormatCutoff\" FROM \"Profiles\"");

            return items.Select(i =>
            {
                i.DeserializedItems = JsonConvert.DeserializeObject<List<ProfileFormatItem147>>(i.FormatItems);
                return i;
            }).ToList();
        }

        [Test]
        public void should_correctly_migrate_custom_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Quality.WEBDL720p, Quality.WEBDL720p, Quality.WEBDL1080p);
            });

            ShouldHaveAddedDefaultFormat(db);
        }

        public class Profile147
        {
            public string FormatItems { get; set; }
            public List<ProfileFormatItem147> DeserializedItems;
            public int FormatCutoff { get; set; }
        }

        public class ProfileFormatItem147
        {
            public int Format;
            public bool Allowed;
        }
    }
}
