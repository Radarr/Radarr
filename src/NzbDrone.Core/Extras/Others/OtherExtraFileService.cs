using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Extras.Others
{
    public interface IOtherExtraFileService : IExtraFileService<OtherExtraFile>
    {
    }

    public class OtherExtraFileService : ExtraFileService<OtherExtraFile>, IOtherExtraFileService
    {
        public OtherExtraFileService(IConfigService configService, IExtraFileRepository<OtherExtraFile> repository, IMovieService movieService, IDiskProvider diskProvider, IRecycleBinProvider recycleBinProvider, Logger logger)
            : base(configService, repository, movieService, diskProvider, recycleBinProvider, logger)
        {
        }

        protected override bool CleanDuringUpgrade(IConfigService configService)
        {
            return !configService.UpgradeKeepOtherFiles;
        }
    }
}
