using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport
{
    public class ImportDecision
    {
        public LocalMovie LocalMovie { get; private set; }
        public IEnumerable<Rejection> Rejections { get; private set; }

        public bool Approved => Rejections.Empty();

        public ImportDecision(LocalMovie localMovie, params Rejection[] rejections)
        {
            LocalMovie = localMovie;
            Rejections = rejections.ToList();
            //LocalMovie = new LocalMovie
            //{
            //    Quality = localMovie.Quality,
            //    ExistingFile = localMovie.ExistingFile,
            //    MediaInfo = localMovie.MediaInfo,
            //    ParsedMovieInfo = localMovie.ParsedMovieInfo,
            //    Path = localMovie.Path,
            //    Size = localMovie.Size
            //};
        }
    }
}
