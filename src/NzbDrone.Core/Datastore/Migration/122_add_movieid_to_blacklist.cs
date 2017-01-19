using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Data;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(122)]
    public class add_movieid_to_blacklist : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Blacklist").AddColumn("MovieId").AsInt32().Nullable().WithDefaultValue(0);
            Alter.Table("Blacklist").AlterColumn("SeriesId").AsInt32().Nullable();
            Alter.Table("Blacklist").AlterColumn("EpisodeIds").AsString().Nullable();
        }

    }
}
