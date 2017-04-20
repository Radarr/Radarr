using NzbDrone.Core.Datastore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public class Compilation : ModelBase
    {
        public Compilation()
        {

        }

        public int CompilationId { get; set; }
        public LazyList<Artist> Artists { get; set; }
    }
}
