﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(71)]
    public class unknown_quality_in_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Weight").FromTable("QualityDefinitions");

            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater70(conn, tran);
            updater.PrependQuality(0);
            updater.Commit();
        }
    }
    public class Profile70
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileItem70> Items { get; set; }
        public int Language { get; set; }
        public List<string> PreferredTags { get; set; }
    }

    public class ProfileItem70
    {
        public int? QualityDefinition { get; set; }
        public int? Quality { get; set; }
        public bool Allowed { get; set; }
    }

    public class QualityDefinition70
    {
        public int Id { get; set; }
        public int Quality { get; set; }
    }

    public class ProfileUpdater70
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile70> _profiles;
        private HashSet<Profile70> _changedProfiles = new HashSet<Profile70>();

        public ProfileUpdater70(IDbConnection conn, IDbTransaction tran)
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
                    updateProfileCmd.CommandText = "UPDATE Profiles SET Name = ?, Cutoff = ?, Items = ?, Language = ? WHERE Id = ?";
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
                if (profile.Items.Any(v => v.Quality == quality)) continue;

                profile.Items.Insert(0, new ProfileItem70
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
                if (profile.Items.Any(v => v.Quality == quality)) continue;

                profile.Items.Add(new ProfileItem70
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
                if (profile.Items.Any(v => v.Quality == quality)) continue;

                var findIndex = profile.Items.FindIndex(v => v.Quality == find);

                profile.Items.Insert(findIndex, new ProfileItem70
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
                if (profile.Items.Any(v => v.Quality == quality)) continue;

                var findIndex = profile.Items.FindIndex(v => v.Quality == find);

                profile.Items.Insert(findIndex + 1, new ProfileItem70
                {
                    Quality = quality,
                    Allowed = false
                });

                _changedProfiles.Add(profile);
            }
        }

        public void UpdateQualityToQualityDefinition()
        {
            var definitions = new List<QualityDefinition70>();
            using (var getDefinitions = _connection.CreateCommand())
            {
                getDefinitions.Transaction = _transaction;
                getDefinitions.CommandText = @"SELECT Id, Quality FROM QualityDefinitions";

                using (var definitionsReader = getDefinitions.ExecuteReader())
                {
                    while (definitionsReader.Read())
                    {
                        int id = definitionsReader.GetInt32(0);
                        int quality = definitionsReader.GetInt32(1);
                        definitions.Add(new QualityDefinition70 {Id = id, Quality = quality});
                    }
                }
            }

            foreach (var profile in _profiles)
            {
                profile.Items = profile.Items.Select(i =>
                {
                    return new ProfileItem70
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

        private List<Profile70> GetProfiles()
        {
            var profiles = new List<Profile70>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT Id, Name, Cutoff, Items, Language FROM Profiles";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile70
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Cutoff = profileReader.GetInt32(2),
                            Items = Json.Deserialize<List<ProfileItem70>>(profileReader.GetString(3)),
                            Language = profileReader.GetInt32(4)
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
