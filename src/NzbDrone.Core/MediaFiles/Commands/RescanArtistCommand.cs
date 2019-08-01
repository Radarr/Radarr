using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RescanArtistCommand : Command
    {
        public int? ArtistId { get; set; }
        public FilterFilesType Filter { get; set; }

        public override bool SendUpdatesToClient => true;

        public RescanArtistCommand(FilterFilesType filter = FilterFilesType.Known)
        {
            Filter = filter;
        }

        public RescanArtistCommand(int artistId, FilterFilesType filter = FilterFilesType.Known)
        {
            ArtistId = artistId;
            Filter = filter;
        }
    }
}
