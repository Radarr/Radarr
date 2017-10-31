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
            var path = Request.Query.Path.Value as string;
            var parsedEpisodeInfo = path.IsNotNullOrWhiteSpace() ? Parser.ParseMusicPath(path) : Parser.ParseMusicTitle(title);

            if (parsedEpisodeInfo == null)
            {
                return null;
            }

            return new ParseResource
            {
                Title = title,
                ParsedAlbumInfo = parsedEpisodeInfo
            };

            //var remoteEpisode = null //_parsingService.Map(parsedEpisodeInfo, 0, 0);

            //if (remoteEpisode != null)
            //{
            //    return new ParseResource
            //    {
            //        Title = title,
            //        ParsedAlbumInfo = remoteEpisode.ParsedEpisodeInfo,
            //        Artist = remoteEpisode.Series.ToResource(),
            //        Albums = remoteEpisode.Episodes.ToResource()
            //    };
            //}
            //else
            //{
            //    return new ParseResource
            //    {
            //        Title = title,
            //        ParsedAlbumInfo = parsedEpisodeInfo
            //    };
            //}
        }
    }
}
