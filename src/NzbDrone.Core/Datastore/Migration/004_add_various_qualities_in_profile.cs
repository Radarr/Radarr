using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(4)]
    public class add_various_qualites_in_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE QualityDefinitions SET Title = 'MP3-160' WHERE Quality = 5"); // Change MP3-512 to MP3-160
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater3(conn, tran);

            updater.AddQuality(Qualities4.WAV);

            updater.MoveQuality(Qualities4.MP3_160, Qualities4.Unknown);

            updater.CreateNewGroup(Qualities4.Unknown, 1000, "Trash Quality Lossy", new[] { Qualities4.MP3_080,
                                                                                            Qualities4.MP3_064,
                                                                                            Qualities4.MP3_056,
                                                                                            Qualities4.MP3_048,
                                                                                            Qualities4.MP3_040,
                                                                                            Qualities4.MP3_032,
                                                                                            Qualities4.MP3_024,
                                                                                            Qualities4.MP3_016,
                                                                                            Qualities4.MP3_008 });

            updater.CreateGroupAt(Qualities4.MP3_160, 1001, "Poor Quality Lossy", new[] { Qualities4.MP3_160,
                                                                                          Qualities4.VORBIS_Q5,
                                                                                          Qualities4.MP3_128,
                                                                                          Qualities4.MP3_096,
                                                                                          Qualities4.MP3_112 }); // Group Vorbis-Q5 with MP3-160

            updater.CreateGroupAt(Qualities4.MP3_192, 1002, "Low Quality Lossy", new[] { Qualities4.MP3_192,
                                                                                         Qualities4.AAC_192,
                                                                                         Qualities4.VORBIS_Q6,
                                                                                         Qualities4.WMA,
                                                                                         Qualities4.MP3_224 }); // Group Vorbis-Q6, AAC 192, WMA with MP3-190

            updater.CreateGroupAt(Qualities4.MP3_256, 1003, "Mid Quality Lossy", new[] { Qualities4.MP3_256,
                                                                                         Qualities4.MP3_VBR_V2,
                                                                                         Qualities4.VORBIS_Q8,
                                                                                         Qualities4.VORBIS_Q7,
                                                                                         Qualities4.AAC_256 }); // Group Mp3-VBR-V2, Vorbis-Q7, Q8, AAC-256 with MP3-256

            updater.CreateGroupAt(Qualities4.MP3_320, 1004, "High Quality Lossy", new[] { Qualities4.MP3_VBR,
                                                                                          Qualities4.MP3_320,
                                                                                          Qualities4.AAC_320,
                                                                                          Qualities4.AAC_VBR,
                                                                                          Qualities4.VORBIS_Q10,
                                                                                          Qualities4.VORBIS_Q9 }); // Group MP3-VBR-V0, AAC-VBR, Vorbis-Q10, Q9, AAC-320 with MP3-320

            updater.CreateGroupAt(Qualities4.FLAC, 1005, "Lossless", new[] { Qualities4.FLAC,
                                                                             Qualities4.ALAC,
                                                                             Qualities4.FLAC_24 }); // Group ALAC with FLAC


            updater.Commit();
        }
    }

    public class Profile4
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileItem4> Items { get; set; }
    }

    public class ProfileItem4
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        public string Name { get; set; }
        public int? Quality { get; set; }
        public List<ProfileItem4> Items { get; set; }
        public bool Allowed { get; set; }

        public ProfileItem4()
        {
            Items = new List<ProfileItem4>();
        }
    }

    public enum Qualities4
    {
        Unknown,
        MP3_192,
        MP3_VBR,
        MP3_256,
        MP3_320,
        MP3_160,
        FLAC,
        ALAC,
        MP3_VBR_V2,
        AAC_192,
        AAC_256,
        AAC_320,
        AAC_VBR,
        WAV,
        VORBIS_Q10,
        VORBIS_Q9,
        VORBIS_Q8,
        VORBIS_Q7,
        VORBIS_Q6,
        VORBIS_Q5,
        WMA,
        FLAC_24,
        MP3_128,
        MP3_096,
        MP3_080,
        MP3_064,
        MP3_056,
        MP3_048,
        MP3_040,
        MP3_032,
        MP3_024,
        MP3_016,
        MP3_008,
        MP3_112,
        MP3_224
    }

    public class ProfileUpdater3
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile4> _profiles;
        private HashSet<Profile4> _changedProfiles = new HashSet<Profile4>();

        public ProfileUpdater3(IDbConnection conn, IDbTransaction tran)
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
                    updateProfileCmd.CommandText =
                        "UPDATE Profiles SET Name = ?, Cutoff = ?, Items = ? WHERE Id = ?";
                    updateProfileCmd.AddParameter(profile.Name);
                    updateProfileCmd.AddParameter(profile.Cutoff);
                    updateProfileCmd.AddParameter(profile.Items.ToJson());
                    updateProfileCmd.AddParameter(profile.Id);

                    updateProfileCmd.ExecuteNonQuery();
                }
            }

            _changedProfiles.Clear();
        }

        public void AddQuality(Qualities4 quality)
        {
            foreach (var profile in _profiles)
            {
                profile.Items.Add(new ProfileItem4
                {
                    Quality = (int)quality,
                    Allowed = false
                });
            }

        }

        public void CreateGroupAt(Qualities4 find, int groupId, string name, Qualities4[] qualities)
        {
            foreach (var profile in _profiles)
            {
                var findIndex = profile.Items.FindIndex(v => v.Quality == (int)find);

                if (findIndex > -1)
                {
                    var findQuality = profile.Items[findIndex];

                    profile.Items.Insert(findIndex, new ProfileItem4
                    {
                        Id = groupId,
                        Name = name,
                        Quality = null,
                        Items = qualities.Select(q => new ProfileItem4
                        {
                            Quality = (int)q,
                            Allowed = findQuality.Allowed
                        }).ToList(),
                        Allowed = findQuality.Allowed
                    });
                }
                else
                {
                    // If the ID isn't found for some reason (mangled migration 71?)

                    profile.Items.Add(new ProfileItem4
                    {
                        Id = groupId,
                        Name = name,
                        Quality = null,
                        Items = qualities.Select(q => new ProfileItem4
                        {
                            Quality = (int)q,
                            Allowed = false
                        }).ToList(),
                        Allowed = false
                    });
                }

                foreach (var quality in qualities)
                {
                    var index = profile.Items.FindIndex(v => v.Quality == (int)quality);

                    if (index > -1)
                    {
                        profile.Items.RemoveAt(index);
                    }

                    if (profile.Cutoff == (int)quality)
                    {
                        profile.Cutoff = groupId;
                    }
                }

                _changedProfiles.Add(profile);
            }
        }

        public void CreateNewGroup(Qualities4 createafter, int groupId, string name, Qualities4[] qualities)
        {
            foreach (var profile in _profiles)
            {
                var findIndex = profile.Items.FindIndex(v => v.Quality == (int)createafter) + 1;

                if (findIndex > -1)
                {

                    profile.Items.Insert(findIndex, new ProfileItem4
                    {
                        Id = groupId,
                        Name = name,
                        Quality = null,
                        Items = qualities.Select(q => new ProfileItem4
                        {
                            Quality = (int)q,
                            Allowed = false
                        }).ToList(),
                        Allowed = false
                    });
                }
                else
                {

                    profile.Items.Add(new ProfileItem4
                    {
                        Id = groupId,
                        Name = name,
                        Quality = null,
                        Items = qualities.Select(q => new ProfileItem4
                        {
                            Quality = (int)q,
                            Allowed = false
                        }).ToList(),
                        Allowed = false
                    });
                }
            }
        }

        public void MoveQuality(Qualities4 quality, Qualities4 moveafter)
        {
            foreach (var profile in _profiles)
            {
                var findIndex = profile.Items.FindIndex(v => v.Quality == (int)quality);

                if (findIndex > -1)
                {
                    var allowed = profile.Items[findIndex].Allowed;
                    profile.Items.RemoveAt(findIndex);
                    var findMoveIndex = profile.Items.FindIndex(v => v.Quality == (int)moveafter) + 1;
                    profile.Items.Insert(findMoveIndex, new ProfileItem4
                    {
                        Quality = (int)quality,
                        Allowed = allowed
                    });
                }


            }
        }

        private List<Profile4> GetProfiles()
        {
            var profiles = new List<Profile4>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT Id, Name, Cutoff, Items FROM Profiles";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile4
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Cutoff = profileReader.GetInt32(2),
                            Items = Json.Deserialize<List<ProfileItem4>>(profileReader.GetString(3))
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
