using System.Collections.Generic;
using NzbDrone.Core.Books;
using NzbDrone.Core.Extras.Files;

namespace NzbDrone.Core.Extras
{
    public interface IImportExistingExtraFiles
    {
        int Order { get; }
        IEnumerable<ExtraFile> ProcessFiles(Author author, List<string> filesOnDisk, List<string> importedFiles);
    }
}
