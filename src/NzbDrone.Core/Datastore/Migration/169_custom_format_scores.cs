using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(169)]
    public class custom_format_scores : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Profiles").AddColumn("MinFormatScore").AsInt32().WithDefaultValue(0);
            Alter.Table("Profiles").AddColumn("CutoffFormatScore").AsInt32().WithDefaultValue(0);

            Execute.WithConnection(MigrateOrderToScores);

            Delete.Column("FormatCutoff").FromTable("Profiles");

            Alter.Table("CustomFormats").AddColumn("IncludeCustomFormatWhenRenaming").AsBoolean().WithDefaultValue(false);
        }

        private void MigrateOrderToScores(IDbConnection conn, IDbTransaction tran)
        {
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<ProfileFormatItem168>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<ProfileFormatItem169>>());

            var rows = conn.Query<Profile168>("SELECT \"Id\", \"FormatCutoff\", \"FormatItems\" from \"Profiles\"", transaction: tran);
            var newRows = new List<Profile169>();

            foreach (var row in rows)
            {
                // Things ranked less than None should have a negative score
                // Things ranked higher than None have a positive score
                var allowedBelowNone = new List<ProfileFormatItem168>();
                var allowedAboveNone = new List<ProfileFormatItem168>();
                var disallowed = new List<ProfileFormatItem168>();

                var noneEnabled = row.FormatItems.Single(x => x.Format == 0).Allowed;

                // If none was disabled, we count everything as above none
                var foundNone = !noneEnabled;
                foreach (var item in row.FormatItems)
                {
                    if (item.Format == 0)
                    {
                        foundNone = true;
                    }
                    else if (!item.Allowed)
                    {
                        disallowed.Add(item);
                    }
                    else if (foundNone)
                    {
                        allowedAboveNone.Add(item);
                    }
                    else
                    {
                        allowedBelowNone.Add(item);
                    }
                }

                // Set up allowed with scores 1, 2, 4, 8 etc so they replicate existing ranking behaviour
                var allowedPositive = allowedAboveNone.Select((x, index) => new ProfileFormatItem169
                {
                    Format = x.Format,
                    Score = (int)Math.Pow(2, index)
                }).ToList();

                // reverse so we have most wanted first
                allowedBelowNone.Reverse();
                var allowedNegative = allowedBelowNone.Select((x, index) => new ProfileFormatItem169
                {
                    Format = x.Format,
                    Score = -1 * (int)Math.Pow(2, index)
                }).ToList();

                // The minimum format score should be the minimum score achievable by the allowed formats
                // By construction, if None disabled then allowedNegative is empty and min is 1
                // If none was enabled, we could have some below None (with negative score) and
                // we should set min score negative to allow for these
                // If someone had no allowed formats and none disabled then keep minScore at 0
                // (This was a broken config that meant nothing would download)
                var minScore = 0;
                if (allowedPositive.Any() && !noneEnabled)
                {
                    minScore = 1;
                }
                else if (allowedNegative.Any())
                {
                    minScore = ((int)Math.Pow(2, allowedNegative.Count) * -1) + 1;
                }

                // Previously anything matching a disabled format was banned from downloading
                // To replicate this, set score negative enough that matching a disabled format
                // must produce a score below the minimum
                var disallowedScore = (-1 * (int)Math.Pow(2, allowedPositive.Count)) + Math.Max(minScore, 0);
                var newDisallowed = disallowed.Select(x => new ProfileFormatItem169
                {
                    Format = x.Format,
                    Score = disallowedScore
                });

                var newItems = newDisallowed.Concat(allowedNegative).Concat(allowedPositive).OrderBy(x => x.Score).ToList();

                // Set the cutoff score to be the score associated with old cutoff format.
                // This can never be achieved by any combination of lesser formats given the 2^n scoring scheme
                // If the cutoff is None (Id == 0) then set cutoff score to zero
                var cutoffScore = 0;
                if (row.FormatCutoff != 0)
                {
                    cutoffScore = newItems.Single(x => x.Format == row.FormatCutoff).Score;
                }

                newRows.Add(new Profile169
                {
                    Id = row.Id,
                    MinFormatScore = minScore,
                    CutoffFormatScore = cutoffScore,
                    FormatItems = newItems
                });
            }

            var sql = $"UPDATE \"Profiles\" SET \"MinFormatScore\" = @MinFormatScore, \"CutoffFormatScore\" = @CutoffFormatScore, \"FormatItems\" = @FormatItems WHERE \"Id\" = @Id";

            conn.Execute(sql, newRows, transaction: tran);
        }

        private class Profile168 : ModelBase
        {
            public int FormatCutoff { get; set; }
            public List<ProfileFormatItem168> FormatItems { get; set; }
        }

        private class ProfileFormatItem168
        {
            public int Format { get; set; }
            public bool Allowed { get; set; }
        }

        private class Profile169 : ModelBase
        {
            public int MinFormatScore { get; set; }
            public int CutoffFormatScore { get; set; }
            public List<ProfileFormatItem169> FormatItems { get; set; }
        }

        private class ProfileFormatItem169
        {
            public int Format { get; set; }
            public int Score { get; set; }
        }
    }
}
