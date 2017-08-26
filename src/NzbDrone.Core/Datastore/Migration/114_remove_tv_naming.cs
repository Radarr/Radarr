using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(114)]
    public class remove_tv_naming : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("RenameEpisodes").FromTable("NamingConfig");
            Delete.Column("StandardEpisodeFormat").FromTable("NamingConfig");
            Delete.Column("DailyEpisodeFormat").FromTable("NamingConfig");
            Delete.Column("AnimeEpisodeFormat").FromTable("NamingConfig");
            Delete.Column("SeasonFolderFormat").FromTable("NamingConfig");
            Delete.Column("SeriesFolderFormat").FromTable("NamingConfig");
            Delete.Column("MultiEpisodeStyle").FromTable("NamingConfig");

            Execute.Sql("DELETE FROM Config WHERE [Key] = 'filedate'");
        }

    }
}
