using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(126)]
    public class update_qualities_and_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater125(conn, tran);
            updater.SplitQualityAppend(0, 27); // TELECINE AFTER Unknown
            updater.SplitQualityAppend(0, 26); // TELESYNC AFTER Unknown
            updater.SplitQualityAppend(0, 25); // CAM AFTER Unknown
            updater.SplitQualityAppend(0, 24); // WORKPRINT AFTER Unknown

            updater.SplitQualityPrepend(2, 23); // DVDR     BEFORE     DVD
            updater.SplitQualityPrepend(2, 28); // DVDSCR   BEFORE     DVD
            updater.SplitQualityPrepend(2, 29); // REGIONAL BEFORE     DVD

            updater.SplitQualityAppend(2, 21); // Bluray576p   AFTER     SDTV
            updater.SplitQualityAppend(2, 20); // Bluray480p   AFTER     SDTV

            updater.AppendQuality(22);

            updater.Commit();
        }
    }

    public class Profile125
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileItem125> Items { get; set; }
        public int Language { get; set; }
        public List<string> PreferredTags { get; set; }
    }

    public class ProfileItem125
    {
        public int? QualityDefinition { get; set; }
        public int? Quality { get; set; }
        public bool Allowed { get; set; }
    }

    public class QualityDefinition125
    {
        public int Id { get; set; }
        public int Quality { get; set; }
    }

    public class ProfileUpdater125
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile125> _profiles;
        private HashSet<Profile125> _changedProfiles = new HashSet<Profile125>();

        public ProfileUpdater125(IDbConnection conn, IDbTransaction tran)
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
                    updateProfileCmd.CommandText = "UPDATE \"Profiles\" SET \"Name\" = ?, \"Cutoff\" = ?, \"Items\" = ?, \"Language\" = ? WHERE \"Id\" = ?";
                    updateProfileCmd.AddParameter(profile.Name);
                    updateProfileCmd.AddParameter(profile.Cutoff);
                    updateProfileCmd.AddParameter(profile.Items.ToJson());
                    updateProfileCmd.AddParameter(profile.Language);
                    updateProfileCmd.AddParameter(profile.Id);

                    updateProfileCmd.ExecuteNonQuery();
                }
            }

            _changedProfiles.Clear();
        }

        public void PrependQuality(int quality)
        {
            foreach (var profile in _profiles)
            {
                if (profile.Items.Any(v => v.Quality == quality))
                {
                    continue;
                }

                profile.Items.Insert(0, new ProfileItem125
                {
                    Quality = quality,
                    Allowed = false
                });

                _changedProfiles.Add(profile);
            }
        }

        public void AppendQuality(int quality)
        {
            foreach (var profile in _profiles)
            {
                if (profile.Items.Any(v => v.Quality == quality))
                {
                    continue;
                }

                profile.Items.Add(new ProfileItem125
                {
                    Quality = quality,
                    Allowed = false
                });

                _changedProfiles.Add(profile);
            }
        }

        public void SplitQualityPrepend(int find, int quality)
        {
            foreach (var profile in _profiles)
            {
                if (profile.Items.Any(v => v.Quality == quality))
                {
                    continue;
                }

                var findIndex = profile.Items.FindIndex(v => v.Quality == find);

                profile.Items.Insert(findIndex, new ProfileItem125
                {
                    Quality = quality,
                    Allowed = profile.Items[findIndex].Allowed
                });

                if (profile.Cutoff == find)
                {
                    profile.Cutoff = quality;
                }

                _changedProfiles.Add(profile);
            }
        }

        public void SplitQualityAppend(int find, int quality)
        {
            foreach (var profile in _profiles)
            {
                if (profile.Items.Any(v => v.Quality == quality))
                {
                    continue;
                }

                var findIndex = profile.Items.FindIndex(v => v.Quality == find);

                profile.Items.Insert(findIndex + 1, new ProfileItem125
                {
                    Quality = quality,
                    Allowed = false
                });

                _changedProfiles.Add(profile);
            }
        }

        public void UpdateQualityToQualityDefinition()
        {
            var definitions = new List<QualityDefinition125>();
            using (var getDefinitions = _connection.CreateCommand())
            {
                getDefinitions.Transaction = _transaction;
                getDefinitions.CommandText = @"SELECT ""Id"", ""Quality"" FROM ""QualityDefinitions""";

                using (var definitionsReader = getDefinitions.ExecuteReader())
                {
                    while (definitionsReader.Read())
                    {
                        int id = definitionsReader.GetInt32(0);
                        int quality = definitionsReader.GetInt32(1);
                        definitions.Add(new QualityDefinition125 { Id = id, Quality = quality });
                    }
                }
            }

            foreach (var profile in _profiles)
            {
                profile.Items = profile.Items.Select(i =>
                {
                    return new ProfileItem125
                    {
                        Allowed = i.Allowed,
                        Quality = i.Quality,
                        QualityDefinition = definitions.Find(d => d.Quality == i.Quality).Id
                    };
                }).ToList();

                profile.Cutoff = definitions.Find(d => d.Quality == profile.Cutoff).Id;

                _changedProfiles.Add(profile);
            }
        }

        private List<Profile125> GetProfiles()
        {
            var profiles = new List<Profile125>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT ""Id"", ""Name"", ""Cutoff"", ""Items"", ""Language"" FROM ""Profiles""";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile125
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Cutoff = profileReader.GetInt32(2),
                            Items = Json.Deserialize<List<ProfileItem125>>(profileReader.GetString(3)),
                            Language = profileReader.GetInt32(4)
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
