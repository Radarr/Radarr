﻿using System.Collections.Generic;
using System.Data;
using FluentMigrator;
 using Marr.Data.QGen;
 using Newtonsoft.Json.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(147)]
    public class add_custom_formats : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            //Execute.WithConnection(RenameUrlToBaseUrl);
            if (!Schema.Table("QualityDefinitions").Column("QualityTags").Exists())
            {
                Alter.Table("QualityDefinitions").AddColumn("QualityTags").AsString().Nullable().WithDefaultValue(null)
                    .AddColumn("ParentQualityDefinition").AsInt64().Nullable().WithDefaultValue(null);
            }
        }

        private void RenameUrlToBaseUrl(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "DELETE FROM AlternativeTitles WHERE rowid NOT IN ( SELECT MIN(rowid) FROM AlternativeTitles GROUP BY CleanTitle )";

                cmd.ExecuteNonQuery();

            }
        }
    }
}
