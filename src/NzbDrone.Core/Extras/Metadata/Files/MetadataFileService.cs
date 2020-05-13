using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Extras.Metadata.Files
{
    public interface IMetadataFileService : IExtraFileService<MetadataFile>
    {
    }

    public class MetadataFileService : ExtraFileService<MetadataFile>, IMetadataFileService
    {
        public MetadataFileService(IExtraFileRepository<MetadataFile> repository, IAuthorService authorService, IDiskProvider diskProvider, IRecycleBinProvider recycleBinProvider, Logger logger)
            : base(repository, authorService, diskProvider, recycleBinProvider, logger)
        {
        }
    }
}
