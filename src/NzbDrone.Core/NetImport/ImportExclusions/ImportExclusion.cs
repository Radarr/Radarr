using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.MediaFiles;
using System.IO;

namespace NzbDrone.Core.NetImport.ImportExclusions
{
    public class ImportExclusion : ModelBase
    {
        public int TmdbId { get; set; }
        public string MovieTitle { get; set; }
        public int MovieYear { get; set; }

        new public string ToString()
        {
            return string.Format("Excluded Movie: [{0}][{1} {2}]", TmdbId, MovieTitle, MovieYear);
        }
    }
}
