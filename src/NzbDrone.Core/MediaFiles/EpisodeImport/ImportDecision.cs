using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public class ImportDecision
    {
        public LocalEpisode LocalEpisode { get; private set; }
        public LocalMovie LocalMovie { get; private set; }
        public IEnumerable<Rejection> Rejections { get; private set; }

        public bool Approved => Rejections.Empty();

        public ImportDecision(LocalEpisode localEpisode, params Rejection[] rejections)
        {
            LocalEpisode = localEpisode;
            Rejections = rejections.ToList();
        }

        public ImportDecision(LocalMovie localMovie, params Rejection[] rejections)
        {
            LocalMovie = localMovie;
            Rejections = rejections.ToList();
            LocalEpisode = new LocalEpisode
            {
                Quality = localMovie.Quality,
                ExistingFile = localMovie.ExistingFile,
                MediaInfo = localMovie.MediaInfo,
                ParsedEpisodeInfo = localMovie.ParsedEpisodeInfo,
                Path = localMovie.Path,
                Size = localMovie.Size
            };
        }
    }
}
