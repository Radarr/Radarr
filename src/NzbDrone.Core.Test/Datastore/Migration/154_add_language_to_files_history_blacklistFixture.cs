using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_language_to_files_history_blacklistFixture : MigrationTest<add_language_to_files_history_blacklist>
    {
        private void AddDefaultProfile(add_language_to_files_history_blacklist m, string name, Language language)
        {
            var allowed = new Quality[] { Quality.WEBDL720p };

            var items = Quality.DefaultQualityDefinitions
                .OrderBy(v => v.Weight)
                .Select(v => new { Quality = (int)v.Quality, Allowed = allowed.Contains(v.Quality) })
                .ToList();

            var profile = new { Id = 1, Name = name, Cutoff = (int)Quality.WEBDL720p, Items = items.ToJson(), Language = (int)language };

            var movie = new
            {
                Id = 1,
                Monitored = true,
                Title = "My Movie",
                CleanTitle = "mytitle",
                Status = (int)MovieStatusType.Announced,
                MinimumAvailability = (int)MovieStatusType.Announced,
                Images = new[] { new { CoverType = "Poster" } }.ToJson(),
                HasPreDBEntry = false,
                PathState = 1,
                Runtime = 90,
                ProfileId = 1,
                MovieFileId = 1,
                Path = "/Some/Path",
                TitleSlug = "123456",
                TmdbId = 123456
            };

            m.Insert.IntoTable("Profiles").Row(profile);
            m.Insert.IntoTable("Movies").Row(movie);
        }

        private void AddMovieFile(add_language_to_files_history_blacklist m, string sceneName, string mediaInfoLanugaes)
        {
            m.Insert.IntoTable("MovieFiles").Row(new
            {
                MovieId = 1,
                Quality = new
                {
                    Quality = 6
                }.ToJson(),
                Size = 997478103,
                DateAdded = DateTime.Now,
                SceneName = sceneName,
                MediaInfo = new
                {
                    AudioLanguages = mediaInfoLanugaes
                }.ToJson(),
                RelativePath = "Never Say Never Again.1983.Bluray-720p.mp4",
            });
        }

        private void AddHistory(add_language_to_files_history_blacklist m, string sourceTitle)
        {
            m.Insert.IntoTable("History").Row(new
            {
                MovieId = 1,
                Quality = new
                {
                    Quality = 6
                }.ToJson(),
                EventType = 1,
                Date = DateTime.Now,
                SourceTitle = sourceTitle,
                Data = new
                {
                    Indexer = "My Indexer"
                }.ToJson()
            });
        }

        private void AddBlacklist(add_language_to_files_history_blacklist m, string sourceTitle)
        {
            m.Insert.IntoTable("Blacklist").Row(new
            {
                MovieId = 1,
                Quality = new
                {
                    Quality = 6
                }.ToJson(),
                Date = DateTime.Now,
                SourceTitle = sourceTitle,
                Size = 997478103
            });
        }

        [Test]
        public void should_add_languages_from_media_info_if_available()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddMovieFile(c, "My.Movie.2018.German.BluRay-Radarr", "Japanese");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"MovieFiles\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.Japanese);
            items.First().Languages.Should().NotContain((int)Language.English);
            items.First().Languages.Should().NotContain((int)Language.Dutch);
        }

        [Test]
        public void should_add_languages_from_media_info_with_multiple_language()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddMovieFile(c, "My.Movie.2018.German.BluRay-Radarr", "Japanese / French");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"MovieFiles\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(2);
            items.First().Languages.Should().Contain((int)Language.Japanese);
            items.First().Languages.Should().Contain((int)Language.French);
            items.First().Languages.Should().NotContain((int)Language.English);
            items.First().Languages.Should().NotContain((int)Language.Dutch);
        }

        [Test]
        public void should_fallback_to_scenename_if_no_mediainfo_languages()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddMovieFile(c, "My.Movie.2018.German.BluRay-Radarr", "");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"MovieFiles\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.German);
            items.First().Languages.Should().NotContain((int)Language.Dutch);
        }

        [Test]
        public void should_fallback_to_scenename_if_mediainfo_language_invalid()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddMovieFile(c, "My.Movie.2018.German.BluRay-Radarr", "English (USA)");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"MovieFiles\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.German);
            items.First().Languages.Should().NotContain((int)Language.Dutch);
            items.First().Languages.Should().NotContain((int)Language.English);
        }

        [Test]
        public void should_fallback_to_profile_if_no_mediainfo_no_scene_name()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddMovieFile(c, "", "");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"MovieFiles\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.Dutch);
            items.First().Languages.Should().NotContain((int)Language.Unknown);
        }

        [Test]
        public void should_handle_if_null_mediainfo_and_null_scenename()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddMovieFile(c, null, null);
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"MovieFiles\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.Dutch);
            items.First().Languages.Should().NotContain((int)Language.Unknown);
        }

        [Test]
        public void should_fallback_to_profile_if_unknown_language_from_scene_name()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddMovieFile(c, "My.Movie.2018.BluRay-Radarr", "");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"MovieFiles\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.Dutch);
            items.First().Languages.Should().NotContain((int)Language.Unknown);
        }

        [Test]
        public void should_use_english_if_fallback_to_profile_and_profile_is_any()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Any);
                AddMovieFile(c, "", "");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"MovieFiles\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.English);
            items.First().Languages.Should().NotContain((int)Language.Any);
            items.First().Languages.Should().NotContain((int)Language.Unknown);
        }

        [Test]
        public void should_assign_history_languages_from_sourceTitle()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddHistory(c, "My.Movie.2018.Italian.BluRay-Radarr");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"History\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.Italian);
            items.First().Languages.Should().NotContain((int)Language.Dutch);
        }

        [Test]
        public void should_assign_history_languages_from_profile_if_no_sourceTitle()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddHistory(c, "");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"History\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.Dutch);
            items.First().Languages.Should().NotContain((int)Language.Unknown);
        }

        [Test]
        public void should_assign_history_languages_from_profile_if_unknown_sourceTitle()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddHistory(c, "Man on Fire");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"History\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.Dutch);
            items.First().Languages.Should().NotContain((int)Language.Unknown);
        }

        [Test]
        public void should_assign_history_languages_from_moviefile_release_mapping_with_mediainfo()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddMovieFile(c, "My.Movie.2018.Italian.BluRay-Radarr", "Italian / French / German");
                AddHistory(c, "My.Movie.2018.Italian.BluRay-Radarr");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"History\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(3);
            items.First().Languages.Should().Contain((int)Language.Italian);
            items.First().Languages.Should().Contain((int)Language.French);
            items.First().Languages.Should().Contain((int)Language.German);
            items.First().Languages.Should().NotContain((int)Language.Dutch);
        }

        [Test]
        public void should_assign_blacklist_languages_from_sourceTitle()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddBlacklist(c, "My.Movie.2018.Italian.BluRay-Radarr");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"Blacklist\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.Italian);
            items.First().Languages.Should().NotContain((int)Language.Dutch);
        }

        [Test]
        public void should_assign_blacklist_languages_from_profile_if_no_sourceTitle()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddBlacklist(c, "");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"Blacklist\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.Dutch);
            items.First().Languages.Should().NotContain((int)Language.Unknown);
        }

        [Test]
        public void should_assign_blacklist_languages_from_profile_if_unknown_sourceTitle()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddDefaultProfile(c, "My Custom Profile", Language.Dutch);
                AddBlacklist(c, "Man on Fire");
            });

            var items = db.Query<ModelWithLanguages154>("SELECT \"Id\", \"Languages\" FROM \"Blacklist\"");

            items.Should().HaveCount(1);
            items.First().Languages.Count.Should().Be(1);
            items.First().Languages.Should().Contain((int)Language.Dutch);
            items.First().Languages.Should().NotContain((int)Language.Unknown);
        }
    }

    public class ModelWithLanguages154
    {
        public int Id { get; set; }
        public List<int> Languages { get; set; }
    }
}
