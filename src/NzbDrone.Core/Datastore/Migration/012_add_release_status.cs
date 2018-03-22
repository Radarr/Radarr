using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(12)]
    public class add_release_status : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("MetadataProfiles").AddColumn("ReleaseStatuses").AsString().WithDefaultValue("");
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater11(conn, tran);
            updater.AddDefaultReleaseStatus();
            updater.Commit();
        }
    }

    public class Profile12
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProfileItem12> ReleaseStatuses { get; set; }
    }

    public class ProfileItem12
    {
        public int ReleaseStatus { get; set; }
        public bool Allowed { get; set; }
    }

    public enum ReleaseStatus12
    {
        Official = 0,
        Promotional = 1,
        Bootleg = 2,
        Pseudo = 3
    }
    
    public class ProfileUpdater11
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile12> _profiles;

        public ProfileUpdater11(IDbConnection conn, IDbTransaction tran)
        {
            _connection = conn;
            _transaction = tran;

            _profiles = GetProfiles();
        }

        public void Commit()
        {
            foreach (var profile in _profiles)
            {
                using (var updateProfileCmd = _connection.CreateCommand())
                {
                    updateProfileCmd.Transaction = _transaction;
                    updateProfileCmd.CommandText =
                        "UPDATE MetadataProfiles SET ReleaseStatuses = ? WHERE Id = ?";
                    updateProfileCmd.AddParameter(profile.ReleaseStatuses.ToJson());
                    updateProfileCmd.AddParameter(profile.Id);

                    updateProfileCmd.ExecuteNonQuery();
                }
            }

            _profiles.Clear();
        }

        public void AddDefaultReleaseStatus()
        {
            foreach (var profile in _profiles)
            {
                profile.ReleaseStatuses = new List<ProfileItem12>
                {
                    new ProfileItem12
                    {
                        ReleaseStatus = (int)ReleaseStatus12.Official,
                        Allowed = true
                    },
                    new ProfileItem12
                    {
                        ReleaseStatus = (int)ReleaseStatus12.Promotional,
                        Allowed = false
                    },
                    new ProfileItem12
                    {
                        ReleaseStatus = (int)ReleaseStatus12.Bootleg,
                        Allowed = false
                    },
                    new ProfileItem12
                    {
                        ReleaseStatus = (int)ReleaseStatus12.Pseudo,
                        Allowed = false
                    }
                };
            }
        }

        private List<Profile12> GetProfiles()
        {
            var profiles = new List<Profile12>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT Id, Name FROM MetadataProfiles";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile12
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1)
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
