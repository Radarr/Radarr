using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MonoTorrent;
using NLog;

namespace NzbDrone.Core.MediaFiles.TorrentInfo
{
    public class TorrentFileInfo
    {
        public int FileCount { get; set; }
        public List<string> Files { get; set; }
        public bool IsSingleFile { get; set; }
        public bool ContainsArchives { get; set; }
        public bool ContainsVideoFile { get; set; }
        public string VideoFileName { get; set; }
        public long TotalSize { get; set; }
    }

    public interface ITorrentFileInfoReader
    {
        string GetHashFromTorrentFile(byte[] fileContents);
        TorrentFileInfo GetTorrentInfo(byte[] fileContents);
    }

    public class TorrentFileInfoReader : ITorrentFileInfoReader
    {
        private readonly Logger _logger;
        private static readonly string[] VideoExtensions = { ".mkv", ".mp4", ".avi", ".mov", ".wmv", ".m4v", ".mpg", ".mpeg", ".ts", ".m2ts" };
        private static readonly string[] ArchiveExtensions = { ".rar", ".zip", ".7z", ".tar", ".gz", ".bz2", ".r00", ".r01", ".r02", ".r03", ".r04", ".r05" };

        public TorrentFileInfoReader(Logger logger)
        {
            _logger = logger;
        }

        public string GetHashFromTorrentFile(byte[] fileContents)
        {
            try
            {
                return Torrent.Load(fileContents).InfoHashes.V1OrV2.ToHex();
            }
            catch
            {
                _logger.Trace("Invalid torrent file contents: {0}", Encoding.ASCII.GetString(fileContents));
                throw;
            }
        }

        public TorrentFileInfo GetTorrentInfo(byte[] fileContents)
        {
            try
            {
                var torrent = Torrent.Load(fileContents);
                var files = torrent.Files.Select(f => f.Path).ToList();
                var fileCount = files.Count;

                var videoFiles = files.Where(f => IsVideoFile(f)).ToList();
                var hasArchives = files.Any(f => IsArchiveFile(f));

                return new TorrentFileInfo
                {
                    FileCount = fileCount,
                    Files = files,
                    IsSingleFile = fileCount == 1,
                    ContainsArchives = hasArchives,
                    ContainsVideoFile = videoFiles.Any(),
                    VideoFileName = videoFiles.FirstOrDefault(),
                    TotalSize = torrent.Size
                };
            }
            catch
            {
                _logger.Trace("Failed to parse torrent file info");
                throw;
            }
        }

        private bool IsVideoFile(string filename)
        {
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            return VideoExtensions.Contains(extension);
        }

        private bool IsArchiveFile(string filename)
        {
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            return ArchiveExtensions.Contains(extension);
        }
    }
}
