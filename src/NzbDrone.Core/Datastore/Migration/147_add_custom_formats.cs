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
                    .AddColumn("ParentQualityDefinitionId").AsInt64().Nullable().WithDefaultValue(null);
            }

            if (Schema.Table("QualityDefinitions").Index("IX_QualityDefinitions_Quality").Exists())
            {
                Alter.Table("QualityDefinitions").AlterColumn("Quality").AsInt64().Nullable();
                Delete.Index("IX_QualityDefinitions_Quality").OnTable("QualityDefinitions");
            }

            Execute.WithConnection(RenameUrlToBaseUrl);
        }

        private void RenameUrlToBaseUrl(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater70(conn, tran);
        }
    }
}
