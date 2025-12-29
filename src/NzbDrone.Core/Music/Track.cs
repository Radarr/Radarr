using NzbDrone.Core.MediaItems;
using NzbDrone.Core.MediaTypes;

namespace NzbDrone.Core.Music
{
    public class Track : MediaItem
    {
        public Track()
        {
            MediaType = MediaType.Music;
        }

        public int? AlbumId { get; set; }
        public string Title { get; set; }
        public string ForeignTrackId { get; set; }
        public int TrackNumber { get; set; }
        public int DiscNumber { get; set; } = 1;
        public int? DurationSeconds { get; set; }

        public override string GetTitle() => Title;
        public override int GetYear() => 0;

        public override string ToString()
        {
            return $"{DiscNumber}-{TrackNumber}: {Title}";
        }
    }
}
