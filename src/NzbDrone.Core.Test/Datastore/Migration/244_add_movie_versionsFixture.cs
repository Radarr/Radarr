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
        private void InsertMovie(add_movie_versions c, int metadataId, int qualityProfileId, string path, int movieFileId)
        {
            c.Insert.IntoTable("Movies").Row(new
            {
                Monitored = true,
                MinimumAvailability = 4,
                QualityProfileId = qualityProfileId,
                MovieFileId = movieFileId,
                MovieMetadataId = metadataId,
                Path = path,
                Added = DateTime.UtcNow,
                Tags = "[]"
            });
        }

        private void InsertMovieFile(add_movie_versions c, int movieId)
        {
            c.Insert.IntoTable("MovieFiles").Row(new
            {
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
            });

            var movies = db.Query<Movie244>("SELECT \"Id\", \"Path\" FROM \"Movies\"");
            var versions = db.Query<MovieVersion244>("SELECT * FROM \"MovieVersions\"");

            versions.Should().HaveCount(2);

            var first = versions.Single(v => v.MovieId == movies.Single(m => m.Path == "/Movies/Movie One (2020)").Id);
            first.QualityProfileId.Should().Be(2);
            first.Path.Should().Be("/Movies/Movie One (2020)");
            first.MovieFileId.Should().Be(10);

            var second = versions.Single(v => v.MovieId == movies.Single(m => m.Path == "/Movies/Movie Two (2021)").Id);
            second.QualityProfileId.Should().Be(3);
            second.MovieFileId.Should().Be(0);
        }

        [Test]
        public void should_leave_existing_movie_files_on_the_primary_version()
        {
            var db = WithMigrationTestDb(c =>
            {
                InsertMovie(c, 1, 2, "/Movies/Movie One (2020)", 1);
                InsertMovieFile(c, 1);
            });

            var files = db.Query<MovieFile244>("SELECT \"Id\", \"MovieVersionId\" FROM \"MovieFiles\"");

            files.Single().MovieVersionId.Should().Be(0);
        }
    }

    public class Movie244
    {
        public int Id { get; set; }
        public string Path { get; set; }
    }

    public class MovieVersion244
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public int MovieFileId { get; set; }
    }

    public class MovieFile244
    {
        public int Id { get; set; }
        public int MovieVersionId { get; set; }
    }
}
