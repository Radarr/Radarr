using System.Collections.Generic;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Extras
{
    public interface IImportExistingExtraFiles
    {
        int Order { get; }
        IEnumerable<ExtraFile> ProcessFiles(Movie movie, List<string> filesOnDisk, List<string> importedFiles, string fileNameBeforeRename);
    }
}
