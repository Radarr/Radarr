using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_movie_versionsFixture : MigrationTest<add_movie_versions>
    {
        private void InsertMovie(add_movie_versions c, int id, int qualityProfileId, string path, int movieFileId)
        {
            c.Insert.IntoTable("Movies").Row(new
            {
                Id = id,
                Monitored = true,
                MinimumAvailability = 4,
                QualityProfileId = qualityProfileId,
                MovieFileId = movieFileId,
                MovieMetadataId = id,
                Path = path,
                Added = DateTime.UtcNow,
                Tags = "[]"
            });
        }

        private void InsertMovieFile(add_movie_versions c, int id, int movieId)
        {
            c.Insert.IntoTable("MovieFiles").Row(new
            {
                Id = id,
                MovieId = movieId,
                RelativePath = "Movie.mkv",
                Quality = new { }.ToJson(),
                IndexerFlags = 0,
                Size = 0,
                DateAdded = DateTime.UtcNow,
                Languages = new List<int> { 1 }.ToJson()
            });
        }

        [Test]
        public void should_create_a_version_for_each_movie()
        {
            var db = WithMigrationTestDb(c =>
            {
                InsertMovie(c, 1, 2, "/Movies/Movie One (2020)", 10);
                InsertMovie(c, 2, 3, "/Movies/Movie Two (2021)", 0);
                InsertMovieFile(c, 10, 1);
            });

            var versions = db.Query<MovieVersion243>("SELECT * FROM \"MovieVersions\" ORDER BY \"MovieId\"");

            versions.Should().HaveCount(2);

            versions[0].MovieId.Should().Be(1);
            versions[0].QualityProfileId.Should().Be(2);
            versions[0].Path.Should().Be("/Movies/Movie One (2020)");
            versions[0].MovieFileId.Should().Be(10);

            versions[1].MovieId.Should().Be(2);
            versions[1].QualityProfileId.Should().Be(3);
            versions[1].MovieFileId.Should().Be(0);
        }

        [Test]
        public void should_link_existing_movie_files_to_their_version()
        {
            var db = WithMigrationTestDb(c =>
            {
                InsertMovie(c, 1, 2, "/Movies/Movie One (2020)", 10);
                InsertMovieFile(c, 10, 1);
                InsertMovieFile(c, 11, 99);
            });

            var version = db.Query<MovieVersion243>("SELECT * FROM \"MovieVersions\"").Single();
            var files = db.Query<MovieFile243>("SELECT \"Id\", \"MovieVersionId\" FROM \"MovieFiles\" ORDER BY \"Id\"");

            files.Single(f => f.Id == 10).MovieVersionId.Should().Be(version.Id);
            files.Single(f => f.Id == 11).MovieVersionId.Should().Be(0);
        }

        [Test]
        public void should_add_quality_profile_to_root_folders()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("RootFolders").Row(new { Path = "/Movies" });
            });

            var folders = db.Query<RootFolder243>("SELECT \"Id\", \"QualityProfileId\" FROM \"RootFolders\"");

            folders.Single().QualityProfileId.Should().Be(0);
        }
    }

    public class MovieVersion243
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public int MovieFileId { get; set; }
    }

    public class MovieFile243
    {
        public int Id { get; set; }
        public int MovieVersionId { get; set; }
    }

    public class RootFolder243
    {
        public int Id { get; set; }
        public int QualityProfileId { get; set; }
    }
}
