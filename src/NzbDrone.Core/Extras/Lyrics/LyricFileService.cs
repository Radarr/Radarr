using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras.Lyrics
{
    public interface ILyricFileService : IExtraFileService<LyricFile>
    {
    }

    public class LyricFileService : ExtraFileService<LyricFile>, ILyricFileService
    {
        public LyricFileService(IExtraFileRepository<LyricFile> repository, IArtistService artistService, IDiskProvider diskProvider, IRecycleBinProvider recycleBinProvider, Logger logger)
            : base(repository, artistService, diskProvider, recycleBinProvider, logger)
        {
        }
    }
}
