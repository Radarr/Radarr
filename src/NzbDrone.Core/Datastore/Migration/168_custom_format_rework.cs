using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(168)]
    public class custom_format_rework : NzbDroneMigrationBase
    {
        private static readonly Regex QualityTagRegex = new Regex(@"^(?<type>R|S|M|E|L|C|I|G)(_((?<m_r>RX)|(?<m_re>RQ)|(?<m_n>N)){0,3})?_(?<value>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SizeTagRegex = new Regex(@"(?<min>\d+(\.\d+)?)\s*<>\s*(?<max>\d+(\.\d+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override void MainDbUpgrade()
        {
            Alter.Table("CustomFormats").AddColumn("Specifications").AsString().WithDefaultValue("[]");

            Execute.WithConnection(UpdateCustomFormats);

            Delete.Column("FormatTags").FromTable("CustomFormats");
        }

        private void UpdateCustomFormats(IDbConnection conn, IDbTransaction tran)
        {
            var existing = conn.Query<FormatTag167>("SELECT \"Id\", \"Name\", \"FormatTags\" FROM \"CustomFormats\"");

            var updated = new List<Specification168>();

            foreach (var row in existing)
            {
                var specs = row.FormatTags.Select(ParseFormatTag).ToList();

                // Use format name for spec if only one spec, otherwise use spec type and a digit
                if (specs.Count == 1)
                {
                    specs[0].Name = row.Name;
                }
                else
                {
                    var groups = specs.GroupBy(x => x.ImplementationName);
                    foreach (var group in groups)
                    {
                        var i = 1;
                        foreach (var spec in group)
                        {
                            spec.Name = $"{spec.ImplementationName} {i}";
                            i++;
                        }
                    }
                }

                updated.Add(new Specification168
                {
                    Id = row.Id,
                    Specifications = specs
                });
            }

            var updateSql = "UPDATE \"CustomFormats\" SET \"Specifications\" = @Specifications WHERE \"Id\" = @Id";
            conn.Execute(updateSql, updated, transaction: tran);
        }

        public ICustomFormatSpecification ParseFormatTag(string raw)
        {
            var match = QualityTagRegex.Match(raw);
            if (!match.Success)
            {
                throw new ArgumentException("Quality Tag is not in the correct format!");
            }

            var result = InitializeSpecification(match);
            result.Negate = match.Groups["m_n"].Success;
            result.Required = match.Groups["m_re"].Success;

            return result;
        }

        private ICustomFormatSpecification InitializeSpecification(Match match)
        {
            var type = match.Groups["type"].Value.ToLower();
            var value = match.Groups["value"].Value.ToLower();
            var isRegex = match.Groups["m_r"].Success;

            switch (type)
            {
                case "r":
                    return new ResolutionSpecification { Value = (int)ParseResolution(value) };
                case "s":
                    return new SourceSpecification { Value = (int)ParseSource(value) };
                case "m":
                    return new QualityModifierSpecification { Value = (int)ParseModifier(value) };
                case "e":
                    return new EditionSpecification { Value = ParseString(value, isRegex) };
                case "l":
                    return new LanguageSpecification { Value = (int)LanguageParser.ParseLanguages(value).First() };
                case "i":
                    return new IndexerFlagSpecification { Value = (int)ParseIndexerFlag(value) };
                case "g":
                    var minMax = ParseSize(value);
                    return new SizeSpecification { Min = minMax.Item1, Max = minMax.Item2 };
                case "c":
                default:
                    return new ReleaseTitleSpecification { Value = ParseString(value, isRegex) };
            }
        }

        private Resolution ParseResolution(string value)
        {
            switch (value)
            {
                case "2160":
                    return Resolution.R2160p;
                case "1080":
                    return Resolution.R1080p;
                case "720":
                    return Resolution.R720p;
                case "576":
                    return Resolution.R576p;
                case "480":
                    return Resolution.R480p;
                default:
                    return Resolution.Unknown;
            }
        }

        private Source ParseSource(string value)
        {
            switch (value)
            {
                case "cam":
                    return Source.CAM;
                case "telesync":
                    return Source.TELESYNC;
                case "telecine":
                    return Source.TELECINE;
                case "workprint":
                    return Source.WORKPRINT;
                case "dvd":
                    return Source.DVD;
                case "tv":
                    return Source.TV;
                case "webdl":
                    return Source.WEBDL;
                case "bluray":
                    return Source.BLURAY;
                default:
                    return Source.UNKNOWN;
            }
        }

        private Modifier ParseModifier(string value)
        {
            switch (value)
            {
                case "regional":
                    return Modifier.REGIONAL;
                case "screener":
                    return Modifier.SCREENER;
                case "rawhd":
                    return Modifier.RAWHD;
                case "brdisk":
                    return Modifier.BRDISK;
                case "remux":
                    return Modifier.REMUX;
                default:
                    return Modifier.NONE;
            }
        }

        private IndexerFlags ParseIndexerFlag(string value)
        {
            var flagValues = Enum.GetValues(typeof(IndexerFlags));

            foreach (IndexerFlags flagValue in flagValues)
            {
                var flagString = flagValue.ToString();
                if (flagString.ToLower().Replace("_", string.Empty) != value.ToLower().Replace("_", string.Empty))
                {
                    continue;
                }

                return flagValue;
            }

            return default;
        }

        private (double, double) ParseSize(string value)
        {
            var matches = SizeTagRegex.Match(value);
            var min = double.Parse(matches.Groups["min"].Value, CultureInfo.InvariantCulture);
            var max = double.Parse(matches.Groups["max"].Value, CultureInfo.InvariantCulture);
            return (min, max);
        }

        private string ParseString(string value, bool isRegex)
        {
            return isRegex ? value : Regex.Escape(value);
        }

        private class FormatTag167 : ModelBase
        {
            public string Name { get; set; }
            public List<string> FormatTags { get; set; }
        }

        private class Specification168 : ModelBase
        {
            public List<ICustomFormatSpecification> Specifications { get; set; }
        }
    }
}
