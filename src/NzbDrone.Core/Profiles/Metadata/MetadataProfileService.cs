using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Lifecycle;
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
        private readonly IImportListFactory _importListFactory;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        public MetadataProfileService(IMetadataProfileRepository profileRepository,
                                      IAuthorService authorService,
                                      IImportListFactory importListFactory,
                                      IRootFolderService rootFolderService,
                                      Logger logger)
        {
            _profileRepository = profileRepository;
            _authorService = authorService;
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

            return FilterBooks(input.Books.Value, seriesLinks, profileId);
        }

        private List<Book> FilterBooks(IEnumerable<Book> books, Dictionary<Book, List<SeriesBookLink>> seriesLinks, int metadataProfileId)
        {
            var profile = Get(metadataProfileId);
            var allowedLanguages = profile.AllowedLanguages.IsNotNullOrWhiteSpace() ? new HashSet<string>(profile.AllowedLanguages.Split(',').Select(x => x.Trim().ToLower())) : new HashSet<string>();

            _logger.Trace($"Filtering:\n{books.Select(x => x.ToString()).Join("\n")}");

            var hash = new HashSet<Book>(books);
            var titles = new HashSet<string>(books.Select(x => x.Title));

            FilterByPredicate(hash, profile, (x, p) => x.Ratings.Votes >= p.MinRatingCount && (double)x.Ratings.Value >= p.MinRating, "rating criteria not met");
            FilterByPredicate(hash, profile, (x, p) => !p.SkipMissingDate || x.ReleaseDate.HasValue, "release date is missing");
            FilterByPredicate(hash, profile, (x, p) => !p.SkipMissingIsbn || x.Isbn13.IsNotNullOrWhiteSpace() || x.Asin.IsNotNullOrWhiteSpace(), "isbn and asin is missing");
            FilterByPredicate(hash, profile, (x, p) => !p.SkipPartsAndSets || !IsPartOrSet(x, seriesLinks.GetValueOrDefault(x), titles), "book is part of set");
            FilterByPredicate(hash, profile, (x, p) => !p.SkipSeriesSecondary || !seriesLinks.ContainsKey(x) || seriesLinks[x].Any(y => y.IsPrimary), "book is a secondary series item");
            FilterByPredicate(hash, profile, (x, p) => !allowedLanguages.Any() || allowedLanguages.Contains(x.Language?.ToLower() ?? "null"), "book language not allowed");

            return hash.ToList();
        }

        private void FilterByPredicate(HashSet<Book> books, MetadataProfile profile, Func<Book, MetadataProfile, bool> bookAllowed, string message)
        {
            var filtered = new HashSet<Book>(books.Where(x => !bookAllowed(x, profile)));
            if (filtered.Any())
            {
                _logger.Trace($"Skipping {filtered.Count} books because {message}:\n{filtered.ConcatToString(x => x.ToString(), "\n")}");
                books.RemoveWhere(x => filtered.Contains(x));
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
                emptyProfile.MinRating == 100)
            {
                return;
            }

            if (!profiles.Any())
            {
                _logger.Info("Setting up standard metadata profile");

                Add(new MetadataProfile
                {
                    Name = "Standard",
                    MinRating = 0,
                    MinRatingCount = 100,
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
                MinRating = 100
            });
        }
    }
}
