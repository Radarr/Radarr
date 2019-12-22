﻿using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(141)]
    public class fix_duplicate_alt_titles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(RenameUrlToBaseUrl);
            Alter.Table("AlternativeTitles").AlterColumn("CleanTitle").AsString().Unique();
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
