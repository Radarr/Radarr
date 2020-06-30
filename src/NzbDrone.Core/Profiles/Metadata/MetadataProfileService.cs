using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Profiles.Metadata
{
    public interface IMetadataProfileService
    {
        MetadataProfile Add(MetadataProfile profile);
        void Update(MetadataProfile profile);
        void Delete(int id);
        List<MetadataProfile> All();
        MetadataProfile Get(int id);
        bool Exists(int id);
        List<Book> FilterBooks(Author input, int profileId);
    }

    public class MetadataProfileService : IMetadataProfileService, IHandle<ApplicationStartedEvent>
    {
        public const string NONE_PROFILE_NAME = "None";

        private static readonly Regex PartOrSetRegex = new Regex(@"(?:\d+ of \d+|\d+/\d+|(?<from>\d+)-(?<to>\d+))");

        private readonly IMetadataProfileRepository _profileRepository;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IImportListFactory _importListFactory;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        public MetadataProfileService(IMetadataProfileRepository profileRepository,
                                      IAuthorService authorService,
                                      IBookService bookService,
                                      IMediaFileService mediaFileService,
                                      IImportListFactory importListFactory,
                                      IRootFolderService rootFolderService,
                                      Logger logger)
        {
            _profileRepository = profileRepository;
            _authorService = authorService;
            _bookService = bookService;
            _mediaFileService = mediaFileService;
            _importListFactory = importListFactory;
            _rootFolderService = rootFolderService;
            _logger = logger;
        }

        public MetadataProfile Add(MetadataProfile profile)
        {
            return _profileRepository.Insert(profile);
        }

        public void Update(MetadataProfile profile)
        {
            if (profile.Name == NONE_PROFILE_NAME)
            {
                throw new InvalidOperationException("Not permitted to alter None metadata profile");
            }

            _profileRepository.Update(profile);
        }

        public void Delete(int id)
        {
            var profile = _profileRepository.Get(id);

            if (profile.Name == NONE_PROFILE_NAME ||
                _authorService.GetAllAuthors().Any(c => c.MetadataProfileId == id) ||
                _importListFactory.All().Any(c => c.MetadataProfileId == id) ||
                _rootFolderService.All().Any(c => c.DefaultMetadataProfileId == id))
            {
                throw new MetadataProfileInUseException(profile.Name);
            }

            _profileRepository.Delete(id);
        }

        public List<MetadataProfile> All()
        {
            return _profileRepository.All().ToList();
        }

        public MetadataProfile Get(int id)
        {
            return _profileRepository.Get(id);
        }

        public bool Exists(int id)
        {
            return _profileRepository.Exists(id);
        }

        public List<Book> FilterBooks(Author input, int profileId)
        {
            var seriesLinks = input.Series.Value.SelectMany(x => x.LinkItems.Value)
                .GroupBy(x => x.Book.Value)
                .ToDictionary(x => x.Key, y => y.ToList());

            var dbAuthor = _authorService.FindById(input.ForeignAuthorId);
            var localBooks = dbAuthor?.Books.Value ?? new List<Book>();
            var localFiles = _mediaFileService.GetFilesByAuthor(dbAuthor?.Id ?? 0);

            return FilterBooks(input.Books.Value, localBooks, localFiles, seriesLinks, profileId);
        }

        private List<Book> FilterBooks(IEnumerable<Book> remoteBooks, List<Book> localBooks, List<BookFile> localFiles, Dictionary<Book, List<SeriesBookLink>> seriesLinks, int metadataProfileId)
        {
            var profile = Get(metadataProfileId);

            _logger.Trace($"Filtering:\n{remoteBooks.Select(x => x.ToString()).Join("\n")}");

            var hash = new HashSet<Book>(remoteBooks);
            var titles = new HashSet<string>(remoteBooks.Select(x => x.Title));

            var localHash = new HashSet<string>(localBooks.Where(x => x.AddOptions.AddType == BookAddType.Manual).Select(x => x.ForeignBookId));
            localHash.UnionWith(localFiles.Select(x => x.Edition.Value.Book.Value.ForeignBookId));

            FilterByPredicate(hash, x => x.ForeignBookId, localHash, profile, (x, p) => (x.Ratings.Popularity >= p.MinPopularity) || x.ReleaseDate > DateTime.UtcNow, "rating criteria not met");
            FilterByPredicate(hash, x => x.ForeignBookId, localHash, profile, (x, p) => !p.SkipMissingDate || x.ReleaseDate.HasValue, "release date is missing");
            FilterByPredicate(hash, x => x.ForeignBookId, localHash, profile, (x, p) => !p.SkipPartsAndSets || !IsPartOrSet(x, seriesLinks.GetValueOrDefault(x), titles), "book is part of set");
            FilterByPredicate(hash, x => x.ForeignBookId, localHash, profile, (x, p) => !p.SkipSeriesSecondary || !seriesLinks.ContainsKey(x) || seriesLinks[x].Any(y => y.IsPrimary), "book is a secondary series item");

            foreach (var book in hash)
            {
                var localEditions = localBooks.SingleOrDefault(x => x.ForeignBookId == book.ForeignBookId)?.Editions.Value ?? new List<Edition>();

                book.Editions = FilterEditions(book.Editions.Value, localEditions, localFiles, profile);
            }

            FilterByPredicate(hash, x => x.ForeignBookId, localHash, profile, (x, p) => x.Editions.Value.Any(), "all editions filterd out");

            return hash.ToList();
        }

        private List<Edition> FilterEditions(IEnumerable<Edition> editions, List<Edition> localEditions, List<BookFile> localFiles, MetadataProfile profile)
        {
            var allowedLanguages = profile.AllowedLanguages.IsNotNullOrWhiteSpace() ? new HashSet<string>(profile.AllowedLanguages.Split(',').Select(x => x.Trim().ToLower())) : new HashSet<string>();

            var hash = new HashSet<Edition>(editions);

            var localHash = new HashSet<string>(localEditions.Where(x => x.ManualAdd).Select(x => x.ForeignEditionId));
            localHash.UnionWith(localFiles.Select(x => x.Edition.Value.ForeignEditionId));

            FilterByPredicate(hash, x => x.ForeignEditionId, localHash, profile, (x, p) => !allowedLanguages.Any() || allowedLanguages.Contains(x.Language?.ToLower() ?? "null"), "edition language not allowed");
            FilterByPredicate(hash, x => x.ForeignEditionId, localHash, profile, (x, p) => !p.SkipMissingIsbn || x.Isbn13.IsNotNullOrWhiteSpace() || x.Asin.IsNotNullOrWhiteSpace(), "isbn and asin is missing");

            return hash.ToList();
        }

        private void FilterByPredicate<T>(HashSet<T> remoteItems, Func<T, string> getId, HashSet<string> localItems, MetadataProfile profile, Func<T, MetadataProfile, bool> bookAllowed, string message)
        {
            var filtered = new HashSet<T>(remoteItems.Where(x => !bookAllowed(x, profile) && !localItems.Contains(getId(x))));
            if (filtered.Any())
            {
                _logger.Trace($"Skipping {filtered.Count} {typeof(T).Name} because {message}:\n{filtered.ConcatToString(x => x.ToString(), "\n")}");
                remoteItems.RemoveWhere(x => filtered.Contains(x));
            }
        }

        private bool IsPartOrSet(Book book, List<SeriesBookLink> seriesLinks, HashSet<string> titles)
        {
            if (seriesLinks != null &&
                seriesLinks.Any(x => x.Position.IsNotNullOrWhiteSpace()) &&
                !seriesLinks.Any(s => double.TryParse(s.Position, out _)))
            {
                // No non-empty series entries parse to a number, so all like 1-3 etc.
                return true;
            }

            // Skip things of form Title1 / Title2 when Title1 and Title2 are already in the list
            var split = book.Title.Split('/').Select(x => x.Trim()).ToList();
            if (split.Count > 1 && split.All(x => titles.Contains(x)))
            {
                return true;
            }

            var match = PartOrSetRegex.Match(book.Title);

            if (match.Groups["from"].Success)
            {
                var from = int.Parse(match.Groups["from"].Value);
                return from >= 1800 && from <= DateTime.UtcNow.Year ? false : true;
            }

            return false;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            var profiles = All();

            // Name is a unique property
            var emptyProfile = profiles.FirstOrDefault(x => x.Name == NONE_PROFILE_NAME);

            // make sure empty profile exists and is actually empty
            // TODO: reinstate
            if (emptyProfile != null &&
                emptyProfile.MinPopularity == 1e10)
            {
                return;
            }

            if (!profiles.Any())
            {
                _logger.Info("Setting up standard metadata profile");

                Add(new MetadataProfile
                {
                    Name = "Standard",
                    MinPopularity = 350,
                    SkipMissingDate = true,
                    SkipPartsAndSets = true,
                    AllowedLanguages = "eng, en-US, en-GB"
                });
            }

            if (emptyProfile != null)
            {
                // emptyProfile is not the correct empty profile - move it out of the way
                _logger.Info($"Renaming non-empty metadata profile {emptyProfile.Name}");

                var names = profiles.Select(x => x.Name).ToList();

                int i = 1;
                emptyProfile.Name = $"{NONE_PROFILE_NAME}.{i}";

                while (names.Contains(emptyProfile.Name))
                {
                    i++;
                }

                _profileRepository.Update(emptyProfile);
            }

            _logger.Info("Setting up empty metadata profile");

            Add(new MetadataProfile
            {
                Name = NONE_PROFILE_NAME,
                MinPopularity = 1e10
            });
        }
    }
}
