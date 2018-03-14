using NLog;
using NzbDrone.Common.Disk;
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
        public SubtitleFileService(IExtraFileRepository<SubtitleFile> repository, IMovieService movieService, IDiskProvider diskProvider, IRecycleBinProvider recycleBinProvider, Logger logger)
            : base(repository, movieService, diskProvider, recycleBinProvider, logger)
        {
        }
    }
}
