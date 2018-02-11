using System.Collections.Generic;

namespace NzbDrone.Core.Qualities
{
    public class QualityTagMatchResult
    {
        public QualityTagMatchResult()
        {
            Matches = new Dictionary<QualityTag, bool>();
        }
        public QualityDefinition QualityDefinition { get; set; }
        public Dictionary<QualityTag, bool> Matches { get; set; }
    }
}
