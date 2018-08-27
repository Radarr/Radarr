using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(19)]
    public class add_ape_quality_in_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater19(conn, tran);

            updater.SplitQualityAppend(6, 35);  // APE after Flac
            updater.SplitQualityAppend(6, 36);  // WavPack after Flac 

            updater.Commit();
        }
    }

    public class Profile19
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileItem19> Items { get; set; }
    }

    public class ProfileItem19
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? Quality { get; set; }

        public bool Allowed { get; set; }
        public List<ProfileItem19> Items { get; set; }
    }

    public class ProfileUpdater19
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile19> _profiles;
        private HashSet<Profile19> _changedProfiles = new HashSet<Profile19>();

        public ProfileUpdater19(IDbConnection conn, IDbTransaction tran)
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
                    updateProfileCmd.CommandText = "UPDATE Profiles SET Name = ?, Cutoff = ?, Items = ? WHERE Id = ?";
                    updateProfileCmd.AddParameter(profile.Name);
                    updateProfileCmd.AddParameter(profile.Cutoff);
                    updateProfileCmd.AddParameter(profile.Items.ToJson());
                    updateProfileCmd.AddParameter(profile.Id);

                    updateProfileCmd.ExecuteNonQuery();
                }
            }

            _changedProfiles.Clear();
        }

        public void SplitQualityAppend(int find, int quality)
        {
            foreach (var profile in _profiles)
            {
                if (profile.Items.Any(v => v.Quality == quality)) continue;

                var findIndex = profile.Items.FindIndex(v =>
                {
                    return v.Quality == find || (v.Items != null && v.Items.Any(b => b.Quality == find));
                });

                profile.Items.Insert(findIndex + 1, new ProfileItem19
                {
                    Quality = quality,
                    Allowed = false
                });

                _changedProfiles.Add(profile);
            }
        }

        private List<Profile19> GetProfiles()
        {
            var profiles = new List<Profile19>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT Id, Name, Cutoff, Items FROM Profiles";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile19
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Cutoff = profileReader.GetInt32(2),
                            Items = Json.Deserialize<List<ProfileItem19>>(profileReader.GetString(3))
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
