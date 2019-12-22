using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MetadataSource.PreDB
{
    public class PreDBSyncCommand : Command
    {

        public override bool SendUpdatesToClient => true;
    }
}
