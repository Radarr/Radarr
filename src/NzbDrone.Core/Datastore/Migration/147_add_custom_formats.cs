using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(147)]
    public class add_custom_formats : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            //Execute.WithConnection(RenameUrlToBaseUrl);
            Create.TableForModel("CustomFormats")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("FormatTags").AsString();

            Alter.Table("Profiles").AddColumn("FormatItems").AsString().WithDefaultValue("[{format:0, allowed:true}]").AddColumn("FormatCutoff").AsInt32().WithDefaultValue(0);

            Execute.WithConnection(AddCustomFormatsToProfile);
        }

        private void AddCustomFormatsToProfile(IDbConnection conn, IDbTransaction tran)
        {

        }
    }
}
