using System;
using NzbDrone.Core.MediaItems;
using NzbDrone.Core.MediaTypes;

namespace NzbDrone.Core.TV
{
    public class Episode : MediaItem
    {
        public Episode()
        {
            MediaType = MediaType.TV;
        }

        public int? TVShowId { get; set; }
        public int? SeasonId { get; set; }

        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }

        public int? SceneSeasonNumber { get; set; }
        public int? SceneEpisodeNumber { get; set; }
        public int? SceneAbsoluteEpisodeNumber { get; set; }

        public string Title { get; set; }
        public string Overview { get; set; }
        public DateTime? AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public int? Runtime { get; set; }

        public bool IsSpecial { get; set; }
        public bool UnverifiedSceneNumbering { get; set; }

        public int? EpisodeFileId { get; set; }

        public override string GetTitle() => Title;
        public override int GetYear() => AirDate?.Year ?? 0;

        public override string ToString()
        {
            return $"S{SeasonNumber:00}E{EpisodeNumber:00} - {Title}";
        }
    }
}
