using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.NetImport
{
    public class NetImportSyncCommand : Command
    {
        public override bool SendUpdatesToClient => true;

        public int ListId = 0;
    }
}
