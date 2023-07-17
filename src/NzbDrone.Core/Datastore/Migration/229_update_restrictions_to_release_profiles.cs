using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(229)]
    public class update_restrictions_to_release_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("Restrictions").To("ReleaseProfiles");

            Alter.Table("ReleaseProfiles").AddColumn("Name").AsString().Nullable().WithDefaultValue(null);
            Alter.Table("ReleaseProfiles").AddColumn("Enabled").AsBoolean().WithDefaultValue(true);
            Alter.Table("ReleaseProfiles").AddColumn("IndexerId").AsInt32().WithDefaultValue(0);
            Delete.Column("Preferred").FromTable("ReleaseProfiles");

            Execute.WithConnection(ChangeRequiredIgnoredTypes);

            Delete.FromTable("ReleaseProfiles").Row(new { Required = "[]", Ignored = "[]" });
        }

        // Update the Required and Ignored columns to be JSON arrays instead of comma separated strings
        private void ChangeRequiredIgnoredTypes(IDbConnection conn, IDbTransaction tran)
        {
            var updatedReleaseProfiles = new List<object>();

            using (var getEmailCmd = conn.CreateCommand())
            {
                getEmailCmd.Transaction = tran;
                getEmailCmd.CommandText = "SELECT \"Id\", \"Required\", \"Ignored\" FROM \"ReleaseProfiles\"";

                using var reader = getEmailCmd.ExecuteReader();

                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var requiredObj = reader.GetValue(1);
                    var ignoredObj = reader.GetValue(2);

                    var required = requiredObj == DBNull.Value
                        ? Enumerable.Empty<string>()
                        : requiredObj.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    var ignored = ignoredObj == DBNull.Value
                        ? Enumerable.Empty<string>()
                        : ignoredObj.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    updatedReleaseProfiles.Add(new
                    {
                        Id = id,
                        Required = required.ToJson(),
                        Ignored = ignored.ToJson()
                    });
                }
            }

            var updateReleaseProfilesSql = "UPDATE \"ReleaseProfiles\" SET \"Required\" = @Required, \"Ignored\" = @Ignored WHERE \"Id\" = @Id";
            conn.Execute(updateReleaseProfilesSql, updatedReleaseProfiles, transaction: tran);
        }
    }
}
