using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(150)]
    public class fix_format_tags_double_underscore : NzbDroneMigrationBase
    {
        public static Regex DoubleUnderscore = new Regex(@"^(?<type>R|S|M|E|L|C|I|G)__(?<value>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertExistingFormatTags);
        }

        private void ConvertExistingFormatTags(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new CustomFormatUpdater149(conn, tran);

            updater.ReplaceInTags(DoubleUnderscore, match =>
            {
                return $"{match.Groups["type"].Value}_{match.Groups["value"].Value}";
            });

            updater.Commit();
        }
    }
}
