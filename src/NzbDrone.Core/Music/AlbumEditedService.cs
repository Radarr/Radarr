using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Music.Events;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Music
{
    public class AlbumEditedService : IHandle<AlbumEditedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly ITrackService _trackService;

        public AlbumEditedService(IManageCommandQueue commandQueueManager,
            ITrackService  trackService)
        {
            _commandQueueManager = commandQueueManager;
            _trackService = trackService;
        }

        public void Handle(AlbumEditedEvent message)
        {
            if (message.Album.AlbumReleases.IsLoaded && message.OldAlbum.AlbumReleases.IsLoaded)
            {
                var new_monitored = new HashSet<int>(message.Album.AlbumReleases.Value.Where(x => x.Monitored).Select(x => x.Id));
                var old_monitored = new HashSet<int>(message.OldAlbum.AlbumReleases.Value.Where(x => x.Monitored).Select(x => x.Id));
                if (!new_monitored.SetEquals(old_monitored) ||
                    (message.OldAlbum.AnyReleaseOk == false && message.Album.AnyReleaseOk == true))
                {
                    // Unlink any old track files
                    var tracks = _trackService.GetTracksByAlbum(message.Album.Id);
                    tracks.ForEach(x => x.TrackFileId = 0);
                    _trackService.SetFileIds(tracks);

                    _commandQueueManager.Push(new RescanArtistCommand(message.Album.ArtistId, FilterFilesType.Matched));
                }
            }
        }
    }
}
