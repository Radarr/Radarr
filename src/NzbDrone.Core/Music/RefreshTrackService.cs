using NLog;
using NzbDrone.Core.MediaFiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Music
{
    public interface IRefreshTrackService
    {
        bool RefreshTrackInfo(List<Track> add, List<Track> update, List<Tuple<Track, Track> > merge, List<Track> delete, List<Track> upToDate, List<Track> remoteTracks, bool forceUpdateFileTags);
    }

    public class RefreshTrackService : IRefreshTrackService
    {
        private readonly ITrackService _trackService;
        private readonly IAudioTagService _audioTagService;
        private readonly Logger _logger;

        public RefreshTrackService(ITrackService trackService,
                                   IAudioTagService audioTagService,
                                   Logger logger)
        {
            _trackService = trackService;
            _audioTagService = audioTagService;
            _logger = logger;
        }

        public bool RefreshTrackInfo(List<Track> add, List<Track> update, List<Tuple<Track, Track> > merge, List<Track> delete, List<Track> upToDate, List<Track> remoteTracks, bool forceUpdateFileTags)
        {
            var updateList = new List<Track>();

            // for tracks that need updating, just grab the remote track and set db ids
            foreach (var track in update)
            {
                var remoteTrack = remoteTracks.Single(e => e.ForeignTrackId == track.ForeignTrackId);
                track.UseMetadataFrom(remoteTrack);

                // make sure title is not null
                track.Title = track.Title ?? "Unknown";
                updateList.Add(track);
            }
                                  
            // Move trackfiles from merged entities into new one
            foreach (var item in merge)
            {
                var trackToMerge = item.Item1;
                var mergeTarget = item.Item2;

                if (mergeTarget.TrackFileId == 0)
                {
                    mergeTarget.TrackFileId = trackToMerge.TrackFileId;
                }

                if (!updateList.Contains(mergeTarget))
                {
                    updateList.Add(mergeTarget);
                }
            }

            _trackService.DeleteMany(delete.Concat(merge.Select(x => x.Item1)).ToList());
            _trackService.UpdateMany(updateList);

            var tagsToUpdate = updateList;
            if (forceUpdateFileTags)
            {
                _logger.Debug("Forcing tag update due to Artist/Album/Release updates");
                tagsToUpdate = updateList.Concat(upToDate).ToList();
            }
            _audioTagService.SyncTags(tagsToUpdate);

            return delete.Any() || updateList.Any() || merge.Any();
        }
    }
}

