using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.TV;

namespace NzbDrone.Core.Parser
{
    public interface ITVParsingService
    {
        ParsedEpisodeInfo ParseEpisodeTitle(string title);
        ParsedEpisodeInfo ParseMinimalPathEpisodeInfo(string path);
        TVShow GetTVShow(string title);
        List<Episode> GetEpisodes(ParsedEpisodeInfo parsedInfo, TVShow tvShow);
    }

    public class TVParsingService : ITVParsingService
    {
        private readonly ITVShowService _tvShowService;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public TVParsingService(
            ITVShowService tvShowService,
            IEpisodeService episodeService,
            Logger logger)
        {
            _tvShowService = tvShowService;
            _episodeService = episodeService;
            _logger = logger;
        }

        public ParsedEpisodeInfo ParseEpisodeTitle(string title)
        {
            return TVParser.ParseEpisodeTitle(title);
        }

        public ParsedEpisodeInfo ParseMinimalPathEpisodeInfo(string path)
        {
            var fileInfo = new FileInfo(path);

            var result = TVParser.ParseEpisodeTitle(fileInfo.Name);

            if (result == null)
            {
                _logger.Debug("Attempting to parse episode info using directory and file names. '{0}'", fileInfo.Directory?.Name);
                result = TVParser.ParseEpisodeTitle(fileInfo.Directory?.Name + " " + fileInfo.Name);
            }

            if (result == null)
            {
                _logger.Debug("Attempting to parse episode info using directory name. '{0}'", fileInfo.Directory?.Name);
                result = TVParser.ParseEpisodeTitle(fileInfo.Directory?.Name + fileInfo.Extension);
            }

            return result;
        }

        public TVShow GetTVShow(string title)
        {
            var parsedInfo = TVParser.ParseEpisodeTitle(title);

            if (parsedInfo?.SeriesTitle.IsNullOrWhiteSpace() == false)
            {
                return _tvShowService.FindByTitle(parsedInfo.SeriesTitle);
            }

            return _tvShowService.FindByTitle(title);
        }

        public List<Episode> GetEpisodes(ParsedEpisodeInfo parsedInfo, TVShow tvShow)
        {
            if (parsedInfo == null || tvShow == null)
            {
                return new List<Episode>();
            }

            if (parsedInfo.FullSeason)
            {
                return _episodeService.GetEpisodesBySeason(tvShow.Id, parsedInfo.SeasonNumber);
            }

            if (parsedInfo.IsDaily && !parsedInfo.AirDate.IsNullOrWhiteSpace())
            {
                var episode = _episodeService.FindByAirDate(tvShow.Id, parsedInfo.AirDate);
                if (episode != null)
                {
                    return new List<Episode> { episode };
                }

                return new List<Episode>();
            }

            if (parsedInfo.IsAbsoluteNumbering && parsedInfo.AbsoluteEpisodeNumbers?.Any() == true)
            {
                return _episodeService.FindByAbsoluteEpisodeNumber(tvShow.Id, parsedInfo.AbsoluteEpisodeNumbers);
            }

            if (parsedInfo.EpisodeNumbers?.Any() == true)
            {
                return _episodeService.FindBySeasonAndEpisode(tvShow.Id, parsedInfo.SeasonNumber, parsedInfo.EpisodeNumbers);
            }

            return new List<Episode>();
        }
    }
}
