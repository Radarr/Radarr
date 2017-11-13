using System;
using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(142)]
    public class update_extra_and_subtitle : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ExtraFiles")
                 .AddColumn("MovieId").AsInt32().Nullable()
                 .AddColumn("MovieFileId").AsInt32().Nullable()
                 .AlterColumn("SeriesId").AsInt32().Nullable()
                 .AlterColumn("SeasonNumber").AsInt32().Nullable()
                 .AlterColumn("EpisodeFileId").AsInt32().Nullable();


            Alter.Table("SubtitleFiles")
                 .AddColumn("MovieId").AsInt32().Nullable()
                 .AddColumn("MovieFileId").AsInt32().Nullable()
                 .AddColumn("SpecialType").AsString().Nullable()
                 .AlterColumn("SeriesId").AsInt32().Nullable()
                 .AlterColumn("SeasonNumber").AsInt32().Nullable()
                 .AlterColumn("EpisodeFileId").AsInt32().Nullable();
        }
    }
}