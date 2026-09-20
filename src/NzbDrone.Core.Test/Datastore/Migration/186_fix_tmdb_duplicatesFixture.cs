using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class fix_tmdb_duplicatesFixture : MigrationTest<fix_tmdb_duplicates>
    {
        private void AddMovie(fix_tmdb_duplicates m, string movieTitle, string titleSlug, int tmdbId, int movieFileId, DateTime? lastInfo, DateTime added)
        {
            var movie = new
            {
                Monitored = true,
                Title = movieTitle,
                CleanTitle = movieTitle,
                Status = (int)MovieStatusType.Announced,
                MinimumAvailability = (int)MovieStatusType.Announced,
                Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                Recommendations = new[] { 1 }.ToJson(),
                HasPreDBEntry = false,
                Runtime = 90,
                OriginalLanguage = 1,
                ProfileId = 1,
                MovieFileId = movieFileId,
                Path = string.Format("/Movies/{0}", movieTitle),
                TitleSlug = titleSlug,
                TmdbId = tmdbId,
                Added = added,
                LastInfoSync = lastInfo,
            };

            m.Insert.IntoTable("Movies").Row(movie);
        }

        [Test]
        public void should_clean_duplicate_movies()
        {
            var tmdbId = 123465;
            var dateAdded = DateTime.UtcNow;

            var db = WithMigrationTestDb(c =>
            {
                AddMovie(c, "movie", "slug", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug1", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug2", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug3", tmdbId, 0, dateAdded, dateAdded);
            });

            var items = db.Query<Movie185>("SELECT \"Id\", \"TmdbId\", \"MovieFileId\" FROM \"Movies\"");

            items.Should().HaveCount(1);
        }

        [Test]
        public void should_not_clean_non_duplicate_movies()
        {
            var tmdbId = 123465;
            var dateAdded = DateTime.UtcNow;

            var db = WithMigrationTestDb(c =>
            {
                AddMovie(c, "movie", "slug", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug1", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug2", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug3", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie2", "slug4", 123457, 0, dateAdded, dateAdded);
            });

            var items = db.Query<Movie185>("SELECT \"Id\", \"TmdbId\", \"MovieFileId\" FROM \"Movies\"");

            items.Should().HaveCount(2);
            items.Where(i => i.TmdbId == tmdbId).Should().HaveCount(1);
        }

        [Test]
        public void should_not_clean_any_if_no_duplicate_movies()
        {
            var dateAdded = DateTime.UtcNow;

            var db = WithMigrationTestDb(c =>
            {
                AddMovie(c, "movie1", "slug", 1, 0, dateAdded, dateAdded);
                AddMovie(c, "movie2", "slug1", 2, 0, dateAdded, dateAdded);
                AddMovie(c, "movie3", "slug2", 3, 0, dateAdded, dateAdded);
                AddMovie(c, "movie4", "slug3", 4, 0, dateAdded, dateAdded);
                AddMovie(c, "movie5", "slug4", 123457, 0, dateAdded, dateAdded);
            });

            var items = db.Query<Movie185>("SELECT \"Id\", \"TmdbId\", \"MovieFileId\" FROM \"Movies\"");

            items.Should().HaveCount(5);
        }

        [Test]
        public void should_keep_movie_with_file_when_duplicates()
        {
            var tmdbId = 123465;
            var dateAdded = DateTime.UtcNow;

            var db = WithMigrationTestDb(c =>
            {
                AddMovie(c, "movie", "slug", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug1", tmdbId, 1, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug2", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug3", tmdbId, 0, dateAdded, dateAdded);
            });

            var items = db.Query<Movie185>("SELECT \"Id\", \"TmdbId\", \"MovieFileId\" FROM \"Movies\"");

            items.Should().HaveCount(1);
            items.First().Id.Should().Be(2);
        }

        [Test]
        public void should_keep_earliest_added_a_movie_with_file_when_duplicates_and_multiple_have_file()
        {
            var tmdbId = 123465;
            var dateAdded = DateTime.UtcNow;

            var db = WithMigrationTestDb(c =>
            {
                AddMovie(c, "movie", "slug", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug1", tmdbId, 1, dateAdded, dateAdded.AddSeconds(200));
                AddMovie(c, "movie", "slug2", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug3", tmdbId, 2, dateAdded, dateAdded);
            });

            var items = db.Query<Movie185>("SELECT \"Id\", \"TmdbId\", \"MovieFileId\" FROM \"Movies\"");

            items.Should().HaveCount(1);
            items.First().MovieFileId.Should().BeGreaterThan(0);
            items.First().Id.Should().Be(4);
        }

        [Test]
        public void should_keep_a_movie_with_info_when_duplicates_and_no_file()
        {
            var tmdbId = 123465;
            var dateAdded = DateTime.UtcNow;

            var db = WithMigrationTestDb(c =>
            {
                AddMovie(c, "movie", "slug", tmdbId, 0, null, dateAdded);
                AddMovie(c, "movie", "slug1", tmdbId, 0, null, dateAdded);
                AddMovie(c, "movie", "slug2", tmdbId, 0, dateAdded, dateAdded);
                AddMovie(c, "movie", "slug3", tmdbId, 0, null, dateAdded);
            });

            var items = db.Query<Movie185>("SELECT \"Id\", \"LastInfoSync\", \"TmdbId\", \"MovieFileId\" FROM \"Movies\"");

            items.Should().HaveCount(1);
            items.First().LastInfoSync.Should().NotBeNull();
        }

        public class Movie185
        {
            public int Id { get; set; }
            public int TmdbId { get; set; }
            public int MovieFileId { get; set; }
            public DateTime? LastInfoSync { get; set; }
        }
    }
}
