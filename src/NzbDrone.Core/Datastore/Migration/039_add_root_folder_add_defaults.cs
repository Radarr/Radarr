using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(39)]
    public class add_root_folder_add_defaults : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("RootFolders").AddColumn("Name").AsString().Nullable();
            Alter.Table("RootFolders").AddColumn("DefaultMetadataProfileId").AsInt32().WithDefaultValue(0);
            Alter.Table("RootFolders").AddColumn("DefaultQualityProfileId").AsInt32().WithDefaultValue(0);
            Alter.Table("RootFolders").AddColumn("DefaultMonitorOption").AsInt32().WithDefaultValue(0);
            Alter.Table("RootFolders").AddColumn("DefaultTags").AsString().Nullable();

            Execute.WithConnection(SetDefaultOptions);
        }

        private void SetDefaultOptions(IDbConnection conn, IDbTransaction tran)
        {
            int metadataId = GetMinProfileId(conn, tran, "MetadataProfiles");
            int qualityId = GetMinProfileId(conn, tran, "QualityProfiles");

            if (metadataId == 0 || qualityId == 0)
            {
                return;
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = $"SELECT Id, Path FROM RootFolders";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var rootFolderId = reader.GetInt32(0);
                        var path = reader.GetString(1);

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE RootFolders SET Name = ?, DefaultMetadataProfileId = ?, DefaultQualityProfileId = ?, DefaultTags = ? WHERE Id = ?";
                            updateCmd.AddParameter(path);
                            updateCmd.AddParameter(metadataId);
                            updateCmd.AddParameter(qualityId);
                            updateCmd.AddParameter("[]");
                            updateCmd.AddParameter(rootFolderId);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private int GetMinProfileId(IDbConnection conn, IDbTransaction tran, string table)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;

                // A plain min(id) will return an empty row if table is empty which is a pain to deal with
                cmd.CommandText = $"SELECT COALESCE(MIN(Id), 0) FROM {table}";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }

                    return 0;
                }
            }
        }
    }
}
