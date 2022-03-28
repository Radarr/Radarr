using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(159)]
    public class add_webrip_qualites : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater159(conn, tran);

            updater.CreateGroupAt(8, "WEB 480p", new int[] { 12 }); // Group WEBRip480p with WEBDL480p
            updater.CreateGroupAt(5, "WEB 720p", new int[] { 14 }); // Group WEBRip720p with WEBDL720p
            updater.CreateGroupAt(3, "WEB 1080p", new int[] { 15 }); // Group WEBRip1080p with WEBDL1080p
            updater.CreateGroupAt(18, "WEB 2160p", new int[] { 17 }); // Group WEBRip2160p with WEBDL2160p

            updater.Commit();
        }
    }

    public class Profile159
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileItem159> Items { get; set; }
    }

    public class ProfileItem159
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        public string Name { get; set; }
        public int? Quality { get; set; }
        public List<ProfileItem159> Items { get; set; }
        public bool Allowed { get; set; }

        public ProfileItem159()
        {
            Items = new List<ProfileItem159>();
        }
    }

    public class ProfileUpdater159
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile159> _profiles;
        private HashSet<Profile159> _changedProfiles = new HashSet<Profile159>();

        public ProfileUpdater159(IDbConnection conn, IDbTransaction tran)
        {
            _connection = conn;
            _transaction = tran;

            _profiles = GetProfiles();
        }

        public void Commit()
        {
            foreach (var profile in _changedProfiles)
            {
                using (var updateProfileCmd = _connection.CreateCommand())
                {
                    updateProfileCmd.Transaction = _transaction;

                    if (_connection.GetType().FullName == "Npgsql.NpgsqlConnection")
                    {
                        updateProfileCmd.CommandText = "UPDATE \"Profiles\" SET \"Name\" = $1, \"Cutoff\" = $2, \"Items\" = $3 WHERE \"Id\" = $4";
                    }
                    else
                    {
                        updateProfileCmd.CommandText = "UPDATE \"Profiles\" SET \"Name\" = ?, \"Cutoff\" = ?, \"Items\" = ? WHERE \"Id\" = ?";
                    }

                    updateProfileCmd.AddParameter(profile.Name);
                    updateProfileCmd.AddParameter(profile.Cutoff);
                    updateProfileCmd.AddParameter(profile.Items.ToJson());
                    updateProfileCmd.AddParameter(profile.Id);

                    updateProfileCmd.ExecuteNonQuery();
                }
            }

            _changedProfiles.Clear();
        }

        public void CreateGroupAt(int find, string name, int[] newQualities)
        {
            foreach (var profile in _profiles)
            {
                var nextGroup = 1000;

                while (true)
                {
                    if (profile.Items.FindIndex(v => v.Id == nextGroup) > -1)
                    {
                        nextGroup++;
                    }
                    else
                    {
                        break;
                    }
                }

                var findIndex = profile.Items.FindIndex(v =>
                {
                    return v.Quality == find || (v.Items != null && v.Items.Any(b => b.Quality == find));
                });

                var isGrouped = !profile.Items.Any(p => p.Quality == find);

                if (findIndex > -1 && !isGrouped)
                {
                    var findQuality = profile.Items[findIndex];

                    var groupItems = new List<ProfileItem159>();

                    foreach (var newQuality in newQualities)
                    {
                        groupItems.Add(new ProfileItem159
                        {
                            Quality = newQuality,
                            Allowed = findQuality.Allowed
                        });
                    }

                    groupItems.Add(new ProfileItem159
                    {
                        Quality = find,
                        Allowed = findQuality.Allowed
                    });

                    profile.Items.Insert(findIndex, new ProfileItem159
                    {
                        Id = nextGroup,
                        Name = name,
                        Quality = null,
                        Items = groupItems,
                        Allowed = findQuality.Allowed
                    });
                }
                else if (findIndex > -1 && isGrouped)
                {
                    var findQuality = profile.Items[findIndex];

                    foreach (var newQuality in newQualities)
                    {
                        profile.Items[findIndex].Items.Insert(0, new ProfileItem159
                        {
                            Quality = newQuality,
                            Allowed = findQuality.Allowed
                        });
                    }
                }
                else
                {
                    // If the ID isn't found for some reason (mangled migration 71?)
                    var groupItems = new List<ProfileItem159>();

                    foreach (var newQuality in newQualities)
                    {
                        groupItems.Add(new ProfileItem159
                        {
                            Quality = newQuality,
                            Allowed = false
                        });
                    }

                    groupItems.Add(new ProfileItem159
                    {
                        Quality = find,
                        Allowed = false
                    });

                    profile.Items.Add(new ProfileItem159
                    {
                        Id = nextGroup,
                        Name = name,
                        Quality = null,
                        Items = groupItems,
                        Allowed = false
                    });
                }

                var cleanQualities = new List<int>();

                cleanQualities.AddRange(newQualities);
                cleanQualities.Add(find);

                foreach (var quality in cleanQualities)
                {
                    var index = profile.Items.FindIndex(v => v.Quality == quality);

                    if (index > -1)
                    {
                        profile.Items.RemoveAt(index);
                    }

                    if (profile.Cutoff == quality)
                    {
                        profile.Cutoff = nextGroup;
                    }
                }

                _changedProfiles.Add(profile);
            }
        }

        private List<Profile159> GetProfiles()
        {
            var profiles = new List<Profile159>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT ""Id"", ""Name"", ""Cutoff"", ""Items"" FROM ""Profiles""";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile159
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Cutoff = profileReader.GetInt32(2),
                            Items = Json.Deserialize<List<ProfileItem159>>(profileReader.GetString(3))
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
