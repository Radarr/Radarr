using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class CutoffUnmetAlbumSearchCommand : Command
    {
        public int? SeriesId { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public CutoffUnmetAlbumSearchCommand()
        {
        }

        public CutoffUnmetAlbumSearchCommand(int seriesId)
        {
            SeriesId = seriesId;
        }
    }
}