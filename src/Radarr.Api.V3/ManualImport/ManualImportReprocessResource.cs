using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Languages;
using Radarr.Api.V3.Movies;
using Radarr.Http.REST;

namespace Radarr.Api.V3.ManualImport
{
    public class ManualImportReprocessResource : RestResource
    {
        public string Path { get; set; }
        public int MovieId { get; set; }
        public MovieResource Movie { get; set; }
        public string DownloadId { get; set; }

        public IEnumerable<Rejection> Rejections { get; set; }
        public List<Language> Languages { get; set; }
    }
}
