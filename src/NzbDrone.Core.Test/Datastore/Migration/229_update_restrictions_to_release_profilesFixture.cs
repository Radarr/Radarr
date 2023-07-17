using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class update_restrictions_to_release_profilesFixture : MigrationTest<update_restrictions_to_release_profiles>
    {
        [Test]
        public void should_migrate_required_ignored_columns_to_json_arrays()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Restrictions").Row(new
                {
                    Required = "x265,1080p",
                    Ignored = "xvid,720p,480p",
                    Tags = new HashSet<int> { }.ToJson()
                });
            });

            var items = db.Query<ReleaseProfile>("SELECT \"Required\", \"Ignored\" FROM \"ReleaseProfiles\"");

            items.Should().HaveCount(1);
            items.First().Required.Should().BeEquivalentTo(new[] { "x265", "1080p" });
            items.First().Ignored.Should().BeEquivalentTo(new[] { "xvid", "720p", "480p" });
        }

        [Test]
        public void should_delete_rows_with_empty_required_ignored_columns()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Restrictions").Row(new
                {
                    Required = "",
                    Ignored = "",
                    Tags = new HashSet<int> { }.ToJson()
                });
            });

            var items = db.Query<ReleaseProfile>("SELECT \"Required\", \"Ignored\" FROM \"ReleaseProfiles\"");

            items.Should().HaveCount(0);
        }
    }
}
