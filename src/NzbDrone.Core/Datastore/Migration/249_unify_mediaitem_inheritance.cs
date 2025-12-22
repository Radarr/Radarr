using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(249)]
    public class unify_mediaitem_inheritance : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
        }
    }
}
