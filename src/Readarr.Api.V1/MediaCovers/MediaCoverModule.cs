using System.IO;
using System.Text.RegularExpressions;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Readarr.Api.V1.MediaCovers
{
    public class MediaCoverModule : ReadarrV1Module
    {
        private const string MEDIA_COVER_AUTHOR_ROUTE = @"/Author/(?<authorId>\d+)/(?<filename>(.+)\.(jpg|png|gif))";
        private const string MEDIA_COVER_BOOK_ROUTE = @"/Book/(?<authorId>\d+)/(?<filename>(.+)\.(jpg|png|gif))";

        private static readonly Regex RegexResizedImage = new Regex(@"-\d+(?=\.(jpg|png|gif)$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        public MediaCoverModule(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider)
            : base("MediaCover")
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;

            Get(MEDIA_COVER_AUTHOR_ROUTE, options => GetAuthorMediaCover(options.authorId, options.filename));
            Get(MEDIA_COVER_BOOK_ROUTE, options => GetBookMediaCover(options.authorId, options.filename));
        }

        private object GetAuthorMediaCover(int authorId, string filename)
        {
            var filePath = Path.Combine(_appFolderInfo.GetAppDataPath(), "MediaCover", authorId.ToString(), filename);

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

        private object GetBookMediaCover(int bookId, string filename)
        {
            var filePath = Path.Combine(_appFolderInfo.GetAppDataPath(), "MediaCover", "Books", bookId.ToString(), filename);

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
