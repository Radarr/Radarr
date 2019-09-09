using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser;
using Lidarr.Api.V1.Albums;
using Lidarr.Api.V1.Artist;
using Lidarr.Http;

namespace Lidarr.Api.V1.Parse
{
    public class ParseModule : LidarrRestModule<ParseResource>
    {
        private readonly IParsingService _parsingService;

        public ParseModule(IParsingService parsingService)
        {
            _parsingService = parsingService;

            GetResourceSingle = Parse;
        }

        private ParseResource Parse()
        {
            var title = Request.Query.Title.Value as string;
            var parsedAlbumInfo = Parser.ParseAlbumTitle(title);

            if (parsedAlbumInfo == null)
            {
                return null;
            }

            var remoteAlbum = _parsingService.Map(parsedAlbumInfo);

            if (remoteAlbum != null)
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedAlbumInfo = remoteAlbum.ParsedAlbumInfo,
                    Artist = remoteAlbum.Artist.ToResource(),
                    Albums = remoteAlbum.Albums.ToResource()
                };
            }
            else
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedAlbumInfo = parsedAlbumInfo
                };
            }
        }
    }
}
