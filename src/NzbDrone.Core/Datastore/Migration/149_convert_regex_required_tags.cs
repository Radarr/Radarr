using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(149)]
    public class convert_regex_required_tags : NzbDroneMigrationBase
    {
        public static Regex OriginalRegex = new Regex(@"^(?<type>R|S|M|E|L|C|I|G)(_((?<m_r>R)|(?<m_re>RE)|(?<m_n>N)){1,3})?_(?<value>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertExistingFormatTags);
        }

        private void ConvertExistingFormatTags(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new CustomFormatUpdater149(conn, tran);

            updater.ReplaceInTags(OriginalRegex, match =>
            {
                var modifiers = "";
                if (match.Groups["m_n"].Success)
                {
                    modifiers += "N";
                }

                if (match.Groups["m_r"].Success)
                {
                    modifiers += "RX";
                }

                if (match.Groups["m_re"].Success)
                {
                    modifiers += "RQ";
                }

                if (!string.IsNullOrEmpty(modifiers))
                {
                    modifiers = "_" + modifiers;
                }

                return $"{match.Groups["type"].Value}{modifiers}_{match.Groups["value"].Value}";
            });

            updater.Commit();
        }
    }

    public class CustomFormat149
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> FormatTags { get; set; }
    }

    public class CustomFormatUpdater149
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<CustomFormat149> _customFormats;
        private HashSet<CustomFormat149> _changedFormats = new HashSet<CustomFormat149>();

        public CustomFormatUpdater149(IDbConnection conn, IDbTransaction tran)
        {
            _connection = conn;
            _transaction = tran;

            _customFormats = GetFormats();
        }

        public void Commit()
        {
            foreach (var profile in _changedFormats)
            {
                using (var updateProfileCmd = _connection.CreateCommand())
                {
                    updateProfileCmd.Transaction = _transaction;
                    if (_connection.GetType().FullName == "Npgsql.NpgsqlConnection")
                    {
                        updateProfileCmd.CommandText = "UPDATE \"CustomFormats\" SET \"Name\" = $1, \"FormatTags\" = $2 WHERE \"Id\" = $3";
                    }
                    else
                    {
                        updateProfileCmd.CommandText = "UPDATE \"CustomFormats\" SET \"Name\" = ?, \"FormatTags\" = ? WHERE \"Id\" = ?";
                    }

                    updateProfileCmd.AddParameter(profile.Name);
                    updateProfileCmd.AddParameter(profile.FormatTags.ToJson());
                    updateProfileCmd.AddParameter(profile.Id);

                    updateProfileCmd.ExecuteNonQuery();
                }
            }

            _changedFormats.Clear();
        }

        public void ReplaceInTags(Regex search, string replacement)
        {
            foreach (var format in _customFormats)
            {
                format.FormatTags.ForEach(t => { search.Replace(t, replacement); });
                _changedFormats.Add(format);
            }
        }

        public void ReplaceInTags(Regex search, MatchEvaluator evaluator)
        {
            foreach (var format in _customFormats)
            {
                format.FormatTags = format.FormatTags.Select(t => search.Replace(t, evaluator)).ToList();
                _changedFormats.Add(format);
            }
        }

        private List<CustomFormat149> GetFormats()
        {
            var profiles = new List<CustomFormat149>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT ""Id"", ""Name"", ""FormatTags"" FROM ""CustomFormats""";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new CustomFormat149
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            FormatTags = Json.Deserialize<List<string>>(profileReader.GetString(2))
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
