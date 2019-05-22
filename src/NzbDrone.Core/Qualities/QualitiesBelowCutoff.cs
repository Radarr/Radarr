using System.Collections.Generic;

namespace NzbDrone.Core.Qualities
{
    public class QualitiesBelowCutoff
    {
        public int ProfileId { get; set; }
        public IEnumerable<int> DoesntMeetCutoffIds { get; set; }
        public IEnumerable<int> MeetsCutoffIds { get; set; }
        public IEnumerable<int> MeetsCustomFormatIds { get; set; }

        public QualitiesBelowCutoff(int profileId, IEnumerable<int> doesntMeetCutoffIds, IEnumerable<int> meetsCutoffIds, IEnumerable<int> meetscustomFormatIds)
        {
            ProfileId = profileId;
            DoesntMeetCutoffIds = doesntMeetCutoffIds;
            MeetsCutoffIds = meetsCutoffIds;
            MeetsCustomFormatIds = meetscustomFormatIds;
        }
    }
}
