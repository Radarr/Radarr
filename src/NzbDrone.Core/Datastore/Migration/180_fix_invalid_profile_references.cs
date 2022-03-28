using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(180)]
    public class fix_invalid_profile_references : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(FixMovies);
        }

        private void FixMovies(IDbConnection conn, IDbTransaction tran)
        {
            var profiles = GetProfileIds(conn);
            var movieRows = conn.Query<ProfileEntity179>($"SELECT \"Id\", \"ProfileId\" FROM \"Movies\"");
            var listRows = conn.Query<ProfileEntity179>($"SELECT \"Id\", \"ProfileId\" FROM \"NetImport\"");

            // Only process if there are lists or movies existing in the DB
            if (movieRows.Any() || listRows.Any())
            {
                //If there are no Profiles lets add the defaults
                if (!profiles.Any())
                {
                    InsertDefaultQualityProfiles(conn, tran);
                    profiles = GetProfileIds(conn);
                }

                var mostCommonProfileId = 0;

                //If we have some movies, lets determine the most common profile used and use it for the bad entries
                if (movieRows.Any())
                {
                    mostCommonProfileId = movieRows.Select(x => x.ProfileId)
                                            .Where(x => profiles.Contains(x))
                                            .GroupBy(p => p)
                                            .OrderByDescending(g => g.Count())
                                            .Select(g => g.Key)
                                            .FirstOrDefault();
                }

                // If all the movie profiles are bad or there are no movies, just use the first profile for bad movies and lsits
                if (mostCommonProfileId == 0)
                {
                    mostCommonProfileId = profiles.First();
                }

                //Correct any Movies that reference profiles that are null
                var sql = $"UPDATE \"Movies\" SET \"ProfileId\" = {mostCommonProfileId} WHERE \"Id\" IN(SELECT \"Movies\".\"Id\" FROM \"Movies\" LEFT OUTER JOIN \"Profiles\" ON \"Movies\".\"ProfileId\" = \"Profiles\".\"Id\" WHERE \"Profiles\".\"Id\" IS NULL)";
                conn.Execute(sql, transaction: tran);

                //Correct any Lists that reference profiles that are null
                sql = $"UPDATE \"NetImport\" SET \"ProfileId\" = {mostCommonProfileId} WHERE \"Id\" IN(SELECT \"NetImport\".\"Id\" FROM \"NetImport\" LEFT OUTER JOIN \"Profiles\" ON \"NetImport\".\"ProfileId\" = \"Profiles\".\"Id\" WHERE \"Profiles\".\"Id\" IS NULL)";
                conn.Execute(sql, transaction: tran);
            }
        }

        private List<int> GetProfileIds(IDbConnection conn)
        {
            return conn.Query<QualityProfile180>("SELECT \"Id\" From \"Profiles\"").Select(p => p.Id).ToList();
        }

        private void InsertDefaultQualityProfiles(IDbConnection conn, IDbTransaction tran)
        {
            var profiles = GetDefaultQualityProfiles(conn);
            var formatItemConverter = new EmbeddedDocumentConverter<List<ProfileFormatItem180>>(new CustomFormatIntConverter());
            var profileItemConverter = new EmbeddedDocumentConverter<List<QualityProfileItem111>>(new QualityIntConverter());
            var profileId = 1;

            foreach (var profile in profiles.OrderBy(p => p.Id))
            {
                using (IDbCommand insertNewLanguageProfileCmd = conn.CreateCommand())
                {
                    insertNewLanguageProfileCmd.Transaction = tran;

                    if (conn.GetType().FullName == "Npgsql.NpgsqlConnection")
                    {
                        insertNewLanguageProfileCmd.CommandText = "INSERT INTO \"Profiles\" (\"Id\", \"Name\", \"Cutoff\", \"Items\", \"Language\", \"FormatItems\", \"MinFormatScore\", \"CutoffFormatScore\", \"UpgradeAllowed\") VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)";
                    }
                    else
                    {
                        insertNewLanguageProfileCmd.CommandText = "INSERT INTO \"Profiles\" (\"Id\", \"Name\", \"Cutoff\", \"Items\", \"Language\", \"FormatItems\", \"MinFormatScore\", \"CutoffFormatScore\", \"UpgradeAllowed\") VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";
                    }

                    insertNewLanguageProfileCmd.AddParameter(profileId);
                    insertNewLanguageProfileCmd.AddParameter(profile.Name);
                    insertNewLanguageProfileCmd.AddParameter(profile.Cutoff);

                    var paramItems = insertNewLanguageProfileCmd.CreateParameter();
                    profileItemConverter.SetValue(paramItems, profile.Items);

                    insertNewLanguageProfileCmd.Parameters.Add(paramItems);
                    insertNewLanguageProfileCmd.AddParameter(profile.Language.Id);

                    var paramFormats = insertNewLanguageProfileCmd.CreateParameter();
                    formatItemConverter.SetValue(paramFormats, profile.FormatItems);

                    insertNewLanguageProfileCmd.Parameters.Add(paramFormats);
                    insertNewLanguageProfileCmd.AddParameter(profile.MinFormatScore);
                    insertNewLanguageProfileCmd.AddParameter(profile.CutoffFormatScore);
                    insertNewLanguageProfileCmd.AddParameter(profile.UpgradeAllowed);

                    insertNewLanguageProfileCmd.ExecuteNonQuery();
                }

                profileId += 1;
            }
        }

        private List<QualityProfile180> GetDefaultQualityProfiles(IDbConnection conn)
        {
            var profiles = new List<QualityProfile180>();

            //Grab custom formats if any exist and add them to the new profiles
            var formats = conn.Query<CustomFormat180>($"SELECT \"Id\" FROM \"CustomFormats\"").ToList();

            profiles.Add(GetDefaultProfile("Any",
                formats,
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
                Quality.WEBRip480p,
                Quality.WEBDL720p,
                Quality.WEBRip720p,
                Quality.WEBDL1080p,
                Quality.WEBRip1080p,
                Quality.WEBDL2160p,
                Quality.WEBRip2160p,
                Quality.Bluray480p,
                Quality.Bluray576p,
                Quality.Bluray720p,
                Quality.Bluray1080p,
                Quality.Bluray2160p,
                Quality.Remux1080p,
                Quality.Remux2160p,
                Quality.BRDISK));

            profiles.Add(GetDefaultProfile("SD",
                formats,
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
                Quality.WEBRip480p,
                Quality.Bluray480p,
                Quality.Bluray576p));

            profiles.Add(GetDefaultProfile("HD-720p",
                formats,
                Quality.Bluray720p,
                Quality.HDTV720p,
                Quality.WEBDL720p,
                Quality.WEBRip720p,
                Quality.Bluray720p));

            profiles.Add(GetDefaultProfile("HD-1080p",
                formats,
                Quality.Bluray1080p,
                Quality.HDTV1080p,
                Quality.WEBDL1080p,
                Quality.WEBRip1080p,
                Quality.Bluray1080p,
                Quality.Remux1080p));

            profiles.Add(GetDefaultProfile("Ultra-HD",
                formats,
                Quality.Remux2160p,
                Quality.HDTV2160p,
                Quality.WEBDL2160p,
                Quality.WEBRip2160p,
                Quality.Bluray2160p,
                Quality.Remux2160p));

            profiles.Add(GetDefaultProfile("HD - 720p/1080p",
                formats,
                Quality.Bluray720p,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.WEBDL720p,
                Quality.WEBRip720p,
                Quality.WEBDL1080p,
                Quality.WEBRip1080p,
                Quality.Bluray720p,
                Quality.Bluray1080p,
                Quality.Remux1080p));

            return profiles;
        }

        private QualityProfile180 GetDefaultProfile(string name, List<CustomFormat180> formats, Quality cutoff = null, params Quality[] allowed)
        {
            var groupedQualites = Quality.DefaultQualityDefinitions.GroupBy(q => q.Weight);
            var items = new List<QualityProfileItem111>();
            var groupId = 1000;
            var profileCutoff = cutoff == null ? Quality.Unknown.Id : cutoff.Id;

            foreach (var group in groupedQualites)
            {
                if (group.Count() == 1)
                {
                    var quality = group.First().Quality;

                    items.Add(new QualityProfileItem111 { Quality = group.First().Quality, Allowed = allowed.Contains(quality), Items = new List<QualityProfileItem111>() });
                    continue;
                }

                var groupAllowed = group.Any(g => allowed.Contains(g.Quality));

                items.Add(new QualityProfileItem111
                {
                    Id = groupId,
                    Name = group.First().GroupName,
                    Items = group.Select(g => new QualityProfileItem111
                    {
                        Quality = g.Quality,
                        Allowed = groupAllowed,
                        Items = new List<QualityProfileItem111>()
                    }).ToList(),
                    Allowed = groupAllowed
                });

                if (group.Any(g => g.Quality.Id == profileCutoff))
                {
                    profileCutoff = groupId;
                }

                groupId++;
            }

            var formatItems = formats.Select(format => new ProfileFormatItem180
            {
                Id = format.Id,
                Score = 0,
                Format = format.Id
            }).ToList();

            var qualityProfile = new QualityProfile180
            {
                Name = name,
                Cutoff = profileCutoff,
                Items = items,
                Language = Language.English,
                MinFormatScore = 0,
                CutoffFormatScore = 0,
                UpgradeAllowed = false,
                FormatItems = formatItems
            };

            return qualityProfile;
        }

        private class ProfileEntity179
        {
            public int Id { get; set; }
            public int ProfileId { get; set; }
        }

        private class QualityProfile180
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Cutoff { get; set; }
            public int MinFormatScore { get; set; }
            public int CutoffFormatScore { get; set; }
            public bool UpgradeAllowed { get; set; }
            public Language Language { get; set; }
            public List<ProfileFormatItem180> FormatItems { get; set; }
            public List<QualityProfileItem111> Items { get; set; }
        }

        private class QualityProfileItem111
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Quality Quality { get; set; }
            public List<QualityProfileItem111> Items { get; set; }
            public bool Allowed { get; set; }
        }

        private class ProfileFormatItem180
        {
            public int Id { get; set; }
            public int Format { get; set; }
            public int Score { get; set; }
        }

        private class CustomFormat180
        {
            public int Id { get; set; }
        }
    }
}
