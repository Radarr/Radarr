﻿using System.Collections.Generic;
using System.Data;
 using System.Linq;
 using FluentMigrator;
 using Marr.Data.QGen;
 using Newtonsoft.Json.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
 using NzbDrone.Core.Qualities;

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

            Execute.WithConnection(AddQualityTagsToDefinitions);
            Execute.WithConnection(RenameUrlToBaseUrl);
        }

        private void AddQualityTagsToDefinitions(IDbConnection conn, IDbTransaction tran)
        {
            foreach (var definition in QualityDefinition.DefaultQualityDefinitions)
            {
                using (var updateDefCmd = conn.CreateCommand())
                {
                    updateDefCmd.Transaction = tran;
                    updateDefCmd.CommandText = "UPDATE QualityDefinitions SET QualityTags = ? WHERE Quality = ?";
                    updateDefCmd.AddParameter(definition.QualityTags.Select(t => t.Raw).ToJson());
                    updateDefCmd.AddParameter((int)definition.Quality);

                    updateDefCmd.ExecuteNonQuery();
                }
            }
        }

        private void RenameUrlToBaseUrl(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater70(conn, tran);
            updater.UpdateQualityToQualityDefinition();
            updater.Commit();
        }
    }

    public class QualityDefinition147
    {
        public int Id { get; set; }
        public int Quality { get; set; }
        public string Title { get; set; }
        public int? MinSize { get; set; }
        public int? MaxSize { get; set; }
        public List<string> QualityTags { get; set; }
        public int? ParentQualityDefinitionId { get; set; }
    }
}
