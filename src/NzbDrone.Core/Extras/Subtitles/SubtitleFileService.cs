using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Extras.Subtitles
{
    public interface ISubtitleFileService : IExtraFileService<SubtitleFile>
    {
    }

    public class SubtitleFileService : ExtraFileService<SubtitleFile>, ISubtitleFileService
    {
        public SubtitleFileService(IConfigService configService, IExtraFileRepository<SubtitleFile> repository, IMovieService movieService, IDiskProvider diskProvider, IRecycleBinProvider recycleBinProvider, Logger logger)
            : base(configService, repository, movieService, diskProvider, recycleBinProvider, logger)
        {
        }

        protected override bool CleanDuringUpgrade(IConfigService configService)
        {
            return !configService.UpgradeKeepSubtitlesFiles;
        }
    }
}
