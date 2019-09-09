using System.IO;
using System.Text.RegularExpressions;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Lidarr.Api.V1.MediaCovers
{
    public class MediaCoverModule : LidarrV1Module
    {
        private static readonly Regex RegexResizedImage = new Regex(@"-\d+(?=\.(jpg|png|gif)$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const string MEDIA_COVER_ARTIST_ROUTE = @"/Artist/(?<artistId>\d+)/(?<filename>(.+)\.(jpg|png|gif))";
        private const string MEDIA_COVER_ALBUM_ROUTE = @"/Album/(?<artistId>\d+)/(?<filename>(.+)\.(jpg|png|gif))";

        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        public MediaCoverModule(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider) : base("MediaCover")
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;

            Get[MEDIA_COVER_ARTIST_ROUTE] = options => GetArtistMediaCover(options.artistId, options.filename);
            Get[MEDIA_COVER_ALBUM_ROUTE] = options => GetAlbumMediaCover(options.artistId, options.filename);
        }

        private Response GetArtistMediaCover(int artistId, string filename)
        {
            var filePath = Path.Combine(_appFolderInfo.GetAppDataPath(), "MediaCover", artistId.ToString(), filename);

            if (!_diskProvider.FileExists(filePath) || _diskProvider.GetFileSize(filePath) == 0)
            {
                // Return the full sized image if someone requests a non-existing resized one.
                // TODO: This code can be removed later once everyone had the update for a while.
                var basefilePath = RegexResizedImage.Replace(filePath, "");
                if (basefilePath == filePath || !_diskProvider.FileExists(basefilePath))
                {
                    return new NotFoundResponse();
                }
                filePath = basefilePath;
            }

            return new StreamResponse(() => File.OpenRead(filePath), MimeTypes.GetMimeType(filePath));
        }

        private Response GetAlbumMediaCover(int albumId, string filename)
        {
            var filePath = Path.Combine(_appFolderInfo.GetAppDataPath(), "MediaCover", "Albums", albumId.ToString(), filename);

            if (!_diskProvider.FileExists(filePath) || _diskProvider.GetFileSize(filePath) == 0)
            {
                // Return the full sized image if someone requests a non-existing resized one.
                // TODO: This code can be removed later once everyone had the update for a while.
                var basefilePath = RegexResizedImage.Replace(filePath, "");
                if (basefilePath == filePath || !_diskProvider.FileExists(basefilePath))
                {
                    return new NotFoundResponse();
                }
                filePath = basefilePath;
            }

            return new StreamResponse(() => File.OpenRead(filePath), MimeTypes.GetMimeType(filePath));
        }
    }
}
