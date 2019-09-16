using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Test.Common;
using System.Linq;
using FluentAssertions;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_artistmetadataid_constraintFixture : MigrationTest<add_artistmetadataid_constraint>
    {
        private string _artistPath = null;

        private void GivenArtistMetadata(add_artistmetadataid_constraint c, int id, string name)
        {
            c.Insert.IntoTable("ArtistMetadata").Row(new
                {
                    Id = id,
                    ForeignArtistId = id,
                    Name = name,
                    Status = 1,
                    Images = "images"
                });
        }
        
        private void GivenArtist(add_artistmetadataid_constraint c, int id, int artistMetadataId, string name)
        {
            _artistPath = $"/mnt/data/path/{name}".AsOsAgnostic();
            c.Insert.IntoTable("Artists").Row(new
                {
                    Id = id,
                    ArtistMetadataId = artistMetadataId,
                    CleanName = name,
                    Path = _artistPath,
                    Monitored = 1,
                    AlbumFolder = 1,
                    LanguageProfileId = 1,
                    MetadataProfileId = 1,
                });
        }
        
        private void VerifyArtists(IDirectDataMapper db, List<int> ids)
        {
            var artists = db.Query("SELECT Artists.* from Artists");

            artists.Select(x => x["Id"]).Should().BeEquivalentTo(ids);

            var duplicates = artists.GroupBy(x => x["ArtistMetadataId"])
                .Where(x => x.Count() > 1);

            duplicates.Should().BeEmpty();
        }
        
        [Test]
        public void migration_031_should_not_remove_unique_artist()
        {
            var db = WithMigrationTestDb(c => {
                    GivenArtistMetadata(c, 1, "test");
                    GivenArtist(c, 1, 1, "test");
                });

            VerifyArtists(db, new List<int> { 1 });
        }

        [Test]
        public void migration_031_should_not_remove_either_unique_artist()
        {
            var db = WithMigrationTestDb(c => {
                    GivenArtistMetadata(c, 1, "test");
                    GivenArtist(c, 1, 1, "test");

                    GivenArtistMetadata(c, 2, "test2");
                    GivenArtist(c, 2, 2, "test2");
                });

            VerifyArtists(db, new List<int> { 1, 2 });
        }

        [Test]
        public void migration_031_should_remove_duplicate_artist()
        {
            var db = WithMigrationTestDb(c => {
                    GivenArtistMetadata(c, 1, "test");
                    GivenArtist(c, 1, 1, "test");

                    GivenArtist(c, 2, 1, "test2");
                });

            VerifyArtists(db, new List<int> { 1 });
        }

        [Test]
        public void migration_031_should_remove_all_duplicate_artists()
        {
            var db = WithMigrationTestDb(c => {
                    GivenArtistMetadata(c, 1, "test");
                    GivenArtist(c, 1, 1, "test");
                    GivenArtist(c, 2, 1, "test");
                    GivenArtist(c, 3, 1, "test");
                    GivenArtist(c, 4, 1, "test");

                    GivenArtistMetadata(c, 2, "test2");
                    GivenArtist(c, 5, 2, "test2");
                    GivenArtist(c, 6, 2, "test2");

                });

            VerifyArtists(db, new List<int> { 1, 5 });
        }
    }
}
