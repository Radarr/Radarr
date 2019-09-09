using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class SameTracksSpecification
    {
        private readonly ITrackService _trackService;

        public SameTracksSpecification(ITrackService trackService)
        {
            _trackService = trackService;
        }

        public bool IsSatisfiedBy(List<Track> tracks)
        {
            var trackIds = tracks.SelectList(e => e.Id);
            var trackFileIds = tracks.Where(c => c.TrackFileId != 0).Select(c => c.TrackFileId).Distinct();

            foreach (var trackFileId in trackFileIds)
            {
                var tracksInFile = _trackService.GetTracksByFileId(trackFileId);

                if (tracksInFile.Select(e => e.Id).Except(trackIds).Any())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
