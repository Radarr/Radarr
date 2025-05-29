using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Extras.Metadata.Files
{
    public interface IMetadataFileService : IExtraFileService<MetadataFile>
    {
    }

    public class MetadataFileService : ExtraFileService<MetadataFile>, IMetadataFileService
    {
        public MetadataFileService(IConfigService configService, IExtraFileRepository<MetadataFile> repository, IMovieService movieService, IDiskProvider diskProvider, IRecycleBinProvider recycleBinProvider, Logger logger)
            : base(configService, repository, movieService, diskProvider, recycleBinProvider, logger)
        {
        }

        protected override bool CleanDuringUpgrade(IConfigService configService)
        {
            return !configService.UpgradeKeepMetadataFiles;
        }
    }
}
