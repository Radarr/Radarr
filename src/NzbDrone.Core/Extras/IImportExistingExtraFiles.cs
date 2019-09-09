using System.Collections.Generic;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras
{
    public interface IImportExistingExtraFiles
    {
        int Order { get; }
        IEnumerable<ExtraFile> ProcessFiles(Artist artist, List<string> filesOnDisk, List<string> importedFiles);
    }
}
