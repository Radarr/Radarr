using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class remove_custom_formats_from_quality_modelFixture : MigrationTest<remove_custom_formats_from_quality_model>
    {
        [Test]
        public void should_remove_custom_format_from_pending_releases()
        {
            var db = WithDapperMigrationTestDb(c =>
                {
                    c.Insert.IntoTable("PendingReleases").Row(new
                    {
                        MovieId = 1,
                        Title = "Test Movie",
                        Added = DateTime.UtcNow,
                        ParsedMovieInfo = @"{
  ""movieTitle"": ""Skyfall"",
  ""simpleReleaseTitle"": ""A Movie (2012) \u002B Extras (1080p BluRay x265 HEVC 10bit DTS 5.1 SAMPA) [QxR]"",
  ""quality"": {
    ""quality"": {
      ""id"": 7,
      ""name"": ""Bluray-1080p"",
      ""source"": ""bluray"",
      ""resolution"": 1080,
      ""modifier"": ""none""
    },
    ""customFormats"": [
      {
        ""name"": ""Standard High Def Surround Sound Movie"",
        ""formatTags"": [
          {
            ""raw"": ""R_1080"",
            ""tagType"": ""resolution"",
            ""tagModifier"": 0,
            ""value"": ""r1080p""
          },
          {
            ""raw"": ""L_English"",
            ""tagType"": ""language"",
            ""tagModifier"": 0,
            ""value"": {
              ""id"": 1,
              ""name"": ""English""
            }
          },
          {
            ""raw"": ""C_DTS"",
            ""tagType"": ""custom"",
            ""tagModifier"": 0,
            ""value"": ""dts""
          }
        ],
        ""id"": 1
      }
    ],
    ""revision"": {
      ""version"": 1,
      ""real"": 0,
      ""isRepack"": false
    },
    ""hardcodedSubs"": null,
    ""qualityDetectionSource"": ""name""
  },
  ""releaseGroup"": ""QxR"",
  ""releaseHash"": """",
  ""edition"": """",
  ""year"": 2012,
  ""imdbId"": """"
}",
                        Release = "{}",
                        Reason = (int)PendingReleaseReason.Delay
                    });
                });

            var json = db.Query<string>("SELECT \"ParsedMovieInfo\" FROM \"PendingReleases\"").First();
            json.Should().NotContain("customFormats");

            var pending = db.Query<ParsedMovieInfo>("SELECT \"ParsedMovieInfo\" FROM \"PendingReleases\"").First();
            pending.Quality.Quality.Should().Be(Quality.Bluray1080p);
            pending.Languages.Should().BeEmpty();
        }

        [Test]
        public void should_fix_quality_for_pending_releases()
        {
            var db = WithDapperMigrationTestDb(c =>
                {
                    c.Insert.IntoTable("PendingReleases").Row(new
                    {
                        MovieId = 1,
                        Title = "Test Movie",
                        Added = DateTime.UtcNow,
                        ParsedMovieInfo = @"{
  ""languages"": [
    ""english""
  ],
  ""movieTitle"": ""Joy"",
  ""simpleReleaseTitle"": ""A Movie.2015.1080p.BluRay.AVC.DTS-HD.MA.5.1-RARBG [f"",
  ""quality"": {
    ""quality"": {
      ""id"": 7,
      ""name"": ""Bluray-1080p"",
      ""source"": ""bluray"",
      ""resolution"": ""r1080P"",
      ""modifier"": ""none""
    },
    ""customFormats"": [],
    ""revision"": {
      ""version"": 1,
      ""real"": 0
    }
  },
  ""releaseGroup"": ""RARBG"",
  ""edition"": """",
  ""year"": 2015,
  ""imdbId"": """"
}",
                        Release = "{}",
                        Reason = (int)PendingReleaseReason.Delay
                    });
                });

            var json = db.Query<string>("SELECT \"ParsedMovieInfo\" FROM \"PendingReleases\"").First();
            json.Should().NotContain("customFormats");
            json.Should().NotContain("resolution");

            var pending = db.Query<ParsedMovieInfo>("SELECT \"ParsedMovieInfo\" FROM \"PendingReleases\"").First();
            pending.Quality.Quality.Should().Be(Quality.Bluray1080p);
            pending.Languages.Should().BeEquivalentTo(new List<Language> { Language.English });
        }
    }
}
