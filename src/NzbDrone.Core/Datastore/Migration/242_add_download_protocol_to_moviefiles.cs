using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(242)]
    public class add_download_protocol_to_moviefiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("MovieFiles").AddColumn("DownloadProtocol").AsInt32().WithDefaultValue(0);
        }
    }
}
