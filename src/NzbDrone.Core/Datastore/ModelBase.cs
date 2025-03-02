using System.Diagnostics;
using Equ;

namespace NzbDrone.Core.Datastore
{
    [DebuggerDisplay("{GetType()} ID = {Id}")]
    public abstract class ModelBase
    {
        [MemberwiseEqualityIgnore]
        public int Id { get; set; }
    }
}
