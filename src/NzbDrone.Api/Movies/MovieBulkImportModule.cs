using System.Collections.Generic;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;
using System.Linq;
using Marr.Data;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Api.Movie
{
    public class MovieBulkImportModule : NzbDroneRestModule<MovieResource>
    {
        private readonly ISearchForNewMovie _searchProxy;
        private readonly IRootFolderService _rootFolderService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IDiskScanService _diskScanService;

        public MovieBulkImportModule(ISearchForNewMovie searchProxy, IRootFolderService rootFolderService, IMakeImportDecision importDecisionMaker, IDiskScanService diskScanService)
            : base("/movies/bulkimport")
        {
            _searchProxy = searchProxy;
            _rootFolderService = rootFolderService;
            _importDecisionMaker = importDecisionMaker;
            _diskScanService = diskScanService;
            Get["/"] = x => Search();
        }


        private Response Search()
        {
            if (Request.Query.Id == 0)
            {
                //Todo error handling
            }

            RootFolder rootFolder = _rootFolderService.Get(Request.Query.Id);

            var parsed = rootFolder.UnmappedFolders.Select(f =>
            {
                Core.Tv.Movie m = null;

                var parsedTitle = Parser.ParseMoviePath(f.Name);
                if (parsedTitle == null)
                {
                    m = new Core.Tv.Movie
                    {
                        Title = f.Name.Replace(".", " ").Replace("-", " "),
                        Path = f.Path,
                    };
                }
                else
                {
                    m = new Core.Tv.Movie
                    {
                        Title = parsedTitle.MovieTitle,
                        Year = parsedTitle.Year,
                        ImdbId = parsedTitle.ImdbId,
                        Path = f.Path
                    };
                }



                var files = _diskScanService.GetVideoFiles(f.Path);

                var decisions = _importDecisionMaker.GetImportDecisions(files.ToList(), m);

                var decision = decisions.Where(d => d.Approved && !d.Rejections.Any()).FirstOrDefault();

                if (decision != null)
                {
                    var local = decision.LocalMovie;

                    m.MovieFile = new LazyLoaded<MovieFile>(new MovieFile
                    {
                        Path = local.Path,
                        Edition = local.ParsedMovieInfo.Edition,
                        Quality = local.Quality,
                        MediaInfo = local.MediaInfo,
                        ReleaseGroup = local.ParsedMovieInfo.ReleaseGroup,
                        RelativePath = f.Path.GetRelativePath(local.Path)
                    });
                }

                return m;
            });

            var mapped = parsed.Select(p =>
            {
                return _searchProxy.MapMovieToTmdbMovie(p);
            });

            return MapToResource(mapped.Where(m => m != null)).AsResponse();
        }


        private static IEnumerable<MovieResource> MapToResource(IEnumerable<Core.Tv.Movie> movies)
        {
            foreach (var currentSeries in movies)
            {
                var resource = currentSeries.ToResource();
                var poster = currentSeries.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                yield return resource;
            }
        }
    }
}