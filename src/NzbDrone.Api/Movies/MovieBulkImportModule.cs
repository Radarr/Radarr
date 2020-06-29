using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.RootFolders;
using Radarr.Http;

namespace NzbDrone.Api.Movies
{
    public class UnmappedComparer : IComparer<UnmappedFolder>
    {
        public int Compare(UnmappedFolder a, UnmappedFolder b)
        {
            return a.Name.CompareTo(b.Name);
        }
    }

    public class MovieBulkImportModule : RadarrRestModule<MovieResource>
    {
        private readonly ISearchForNewMovie _searchProxy;
        private readonly IRootFolderService _rootFolderService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IDiskScanService _diskScanService;
        private readonly ICached<Core.Movies.Movie> _mappedMovies;
        private readonly IParsingService _parsingService;
        private readonly IProfileService _profileService;
        private readonly IMovieService _movieService;

        public MovieBulkImportModule(ISearchForNewMovie searchProxy,
            IRootFolderService rootFolderService,
            IMakeImportDecision importDecisionMaker,
            IDiskScanService diskScanService,
            ICacheManager cacheManager,
            IParsingService parsingService,
            IProfileService profileService,
            IMovieService movieService)
            : base("/movies/bulkimport")
        {
            _searchProxy = searchProxy;
            _rootFolderService = rootFolderService;
            _importDecisionMaker = importDecisionMaker;
            _diskScanService = diskScanService;
            _mappedMovies = cacheManager.GetCache<Core.Movies.Movie>(GetType(), "mappedMoviesCache");
            _movieService = movieService;
            _profileService = profileService;
            _parsingService = parsingService;
            Get("/", x => Search());
        }

        private object Search()
        {
            if (Request.Query.Id == 0)
            {
                //Todo error handling
            }

            Profile tempProfile = _profileService.All().First();

            RootFolder rootFolder = _rootFolderService.Get(Request.Query.Id, true);

            int page = Request.Query.page;
            int per_page = Request.Query.per_page;

            int min = (page - 1) * per_page;

            int max = page * per_page;

            var unmapped = rootFolder.UnmappedFolders.OrderBy(f => f.Name).ToList();

            int total_count = unmapped.Count;

            if (Request.Query.total_entries.HasValue)
            {
                total_count = Request.Query.total_entries;
            }

            max = total_count >= max ? max : total_count;

            var paged = unmapped.GetRange(min, max - min);

            var mapped = paged.Select(f =>
            {
                Core.Movies.Movie m = null;

                var mappedMovie = _mappedMovies.Find(f.Name);

                if (mappedMovie != null)
                {
                    return mappedMovie;
                }

                var parsedTitle = _parsingService.ParseMinimalPathMovieInfo(f.Name);
                if (parsedTitle == null)
                {
                    m = new Core.Movies.Movie
                    {
                        Title = f.Name.Replace(".", " ").Replace("-", " "),
                        Path = f.Path,
                        Profile = tempProfile
                    };
                }
                else
                {
                    parsedTitle.ImdbId = Parser.ParseImdbId(parsedTitle.SimpleReleaseTitle);

                    m = new Core.Movies.Movie
                    {
                        Title = parsedTitle.MovieTitle,
                        Year = parsedTitle.Year,
                        ImdbId = parsedTitle.ImdbId,
                        Path = f.Path,
                        Profile = tempProfile
                    };
                }

                var files = _diskScanService.GetVideoFiles(f.Path);

                var decisions = _importDecisionMaker.GetImportDecisions(files.ToList(), m);

                var decision = decisions.Where(d => d.Approved && !d.Rejections.Any()).FirstOrDefault();

                if (decision != null)
                {
                    var local = decision.LocalMovie;

                    m.MovieFile = new MovieFile
                    {
                        Path = local.Path,
                        Edition = local.Edition,
                        Quality = local.Quality,
                        MediaInfo = local.MediaInfo,
                        ReleaseGroup = local.ReleaseGroup,
                        RelativePath = f.Path.GetRelativePath(local.Path)
                    };
                }

                mappedMovie = _searchProxy.MapMovieToTmdbMovie(m);

                if (mappedMovie != null)
                {
                    mappedMovie.Monitored = true;

                    _mappedMovies.Set(f.Name, mappedMovie, TimeSpan.FromDays(2));

                    return mappedMovie;
                }

                return null;
            });

            return new PagingResource<MovieResource>
            {
                Page = page,
                PageSize = per_page,
                SortDirection = SortDirection.Ascending,
                SortKey = Request.Query.sort_by,
                TotalRecords = total_count - mapped.Where(m => m == null).Count(),
                Records = MapToResource(mapped.Where(m => m != null)).ToList()
            };
        }

        private static IEnumerable<MovieResource> MapToResource(IEnumerable<Core.Movies.Movie> movies)
        {
            foreach (var currentMovie in movies)
            {
                var resource = currentMovie.ToResource();
                var poster = currentMovie.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                yield return resource;
            }
        }
    }
}
