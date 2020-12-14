using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Books
{
    public interface IAuthorService
    {
        Author GetAuthor(int authorId);
        Author GetAuthorByMetadataId(int authorMetadataId);
        List<Author> GetAuthors(IEnumerable<int> authorIds);
        Author AddAuthor(Author newAuthor, bool doRefresh);
        List<Author> AddAuthors(List<Author> newAuthors, bool doRefresh);
        Author FindById(string foreignAuthorId);
        Author FindByName(string title);
        Author FindByNameInexact(string title);
        List<Author> GetCandidates(string title);
        List<Author> GetReportCandidates(string reportTitle);
        void DeleteAuthor(int authorId, bool deleteFiles, bool addImportListExclusion = false);
        List<Author> GetAllAuthors();
        List<Author> AllForTag(int tagId);
        Author UpdateAuthor(Author author);
        List<Author> UpdateAuthors(List<Author> authors, bool useExistingRelativeFolder);
        Dictionary<int, string> AllAuthorPaths();
        bool AuthorPathExists(string folder);
        void RemoveAddOptions(Author author);
    }

    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildAuthorPaths _authorPathBuilder;
        private readonly Logger _logger;
        private readonly ICached<List<Author>> _cache;

        public AuthorService(IAuthorRepository authorRepository,
                             IEventAggregator eventAggregator,
                             IBuildAuthorPaths authorPathBuilder,
                             ICacheManager cacheManager,
                             Logger logger)
        {
            _authorRepository = authorRepository;
            _eventAggregator = eventAggregator;
            _authorPathBuilder = authorPathBuilder;
            _cache = cacheManager.GetCache<List<Author>>(GetType());
            _logger = logger;
        }

        public Author AddAuthor(Author newAuthor, bool doRefresh)
        {
            _cache.Clear();
            _authorRepository.Insert(newAuthor);
            _eventAggregator.PublishEvent(new AuthorAddedEvent(GetAuthor(newAuthor.Id), doRefresh));

            return newAuthor;
        }

        public List<Author> AddAuthors(List<Author> newAuthors, bool doRefresh)
        {
            _cache.Clear();
            _authorRepository.InsertMany(newAuthors);
            _eventAggregator.PublishEvent(new AuthorsImportedEvent(newAuthors.Select(s => s.Id).ToList(), doRefresh));

            return newAuthors;
        }

        public bool AuthorPathExists(string folder)
        {
            return _authorRepository.AuthorPathExists(folder);
        }

        public void DeleteAuthor(int authorId, bool deleteFiles, bool addImportListExclusion = false)
        {
            _cache.Clear();
            var author = _authorRepository.Get(authorId);
            _authorRepository.Delete(authorId);
            _eventAggregator.PublishEvent(new AuthorDeletedEvent(author, deleteFiles, addImportListExclusion));
        }

        public Author FindById(string foreignAuthorId)
        {
            return _authorRepository.FindById(foreignAuthorId);
        }

        public Author FindByName(string title)
        {
            return _authorRepository.FindByName(title.CleanAuthorName());
        }

        public List<Tuple<Func<Author, string, double>, string>> AuthorScoringFunctions(string title, string cleanTitle)
        {
            Func<Func<Author, string, double>, string, Tuple<Func<Author, string, double>, string>> tc = Tuple.Create;
            var scoringFunctions = new List<Tuple<Func<Author, string, double>, string>>
            {
                tc((a, t) => a.CleanName.FuzzyMatch(t), cleanTitle),
                tc((a, t) => a.Name.FuzzyMatch(t), title),
                tc((a, t) => a.Metadata.Value.Aliases.Concat(new List<string> { a.Name }).Max(x => x.CleanAuthorName().FuzzyMatch(t)), cleanTitle),
            };

            if (title.StartsWith("The ", StringComparison.CurrentCultureIgnoreCase))
            {
                scoringFunctions.Add(tc((a, t) => a.CleanName.FuzzyMatch(t), title.Substring(4).CleanAuthorName()));
            }
            else
            {
                scoringFunctions.Add(tc((a, t) => a.CleanName.FuzzyMatch(t), "the" + cleanTitle));
            }

            return scoringFunctions;
        }

        public Author FindByNameInexact(string title)
        {
            var authors = GetAllAuthors();

            foreach (var func in AuthorScoringFunctions(title, title.CleanAuthorName()))
            {
                var results = FindByStringInexact(authors, func.Item1, func.Item2);
                if (results.Count == 1)
                {
                    return results[0];
                }
            }

            return null;
        }

        public List<Author> GetCandidates(string title)
        {
            var authors = GetAllAuthors();
            var output = new List<Author>();

            foreach (var func in AuthorScoringFunctions(title, title.CleanAuthorName()))
            {
                output.AddRange(FindByStringInexact(authors, func.Item1, func.Item2));
            }

            return output.DistinctBy(x => x.Id).ToList();
        }

        public List<Tuple<Func<Author, string, double>, string>> ReportAuthorScoringFunctions(string reportTitle, string cleanReportTitle)
        {
            Func<Func<Author, string, double>, string, Tuple<Func<Author, string, double>, string>> tc = Tuple.Create;
            var scoringFunctions = new List<Tuple<Func<Author, string, double>, string>>
            {
                tc((a, t) => t.FuzzyContains(a.CleanName), cleanReportTitle),
                tc((a, t) => t.FuzzyContains(a.Metadata.Value.Name), reportTitle)
            };

            return scoringFunctions;
        }

        public List<Author> GetReportCandidates(string reportTitle)
        {
            var authors = GetAllAuthors();
            var output = new List<Author>();

            foreach (var func in AuthorScoringFunctions(reportTitle, reportTitle.CleanAuthorName()))
            {
                output.AddRange(FindByStringInexact(authors, func.Item1, func.Item2));
            }

            return output.DistinctBy(x => x.Id).ToList();
        }

        private List<Author> FindByStringInexact(List<Author> authors, Func<Author, string, double> scoreFunction, string title)
        {
            const double fuzzThreshold = 0.8;
            const double fuzzGap = 0.2;

            var sortedAuthors = authors.Select(s => new
            {
                MatchProb = scoreFunction(s, title),
                Author = s
            })
                .ToList()
                .OrderByDescending(s => s.MatchProb)
                .ToList();

            return sortedAuthors.TakeWhile((x, i) => i == 0 || sortedAuthors[i - 1].MatchProb - x.MatchProb < fuzzGap)
                .TakeWhile((x, i) => x.MatchProb > fuzzThreshold || (i > 0 && sortedAuthors[i - 1].MatchProb > fuzzThreshold))
                .Select(x => x.Author)
                .ToList();
        }

        public List<Author> GetAllAuthors()
        {
            return _cache.Get("GetAllAuthors", () => _authorRepository.All().ToList(), TimeSpan.FromSeconds(30));
        }

        public Dictionary<int, string> AllAuthorPaths()
        {
            return _authorRepository.AllAuthorPaths();
        }

        public List<Author> AllForTag(int tagId)
        {
            return GetAllAuthors().Where(s => s.Tags.Contains(tagId))
                                 .ToList();
        }

        public Author GetAuthor(int authorId)
        {
            return _authorRepository.Get(authorId);
        }

        public Author GetAuthorByMetadataId(int authorMetadataId)
        {
            return _authorRepository.GetAuthorByMetadataId(authorMetadataId);
        }

        public List<Author> GetAuthors(IEnumerable<int> authorIds)
        {
            return _authorRepository.Get(authorIds).ToList();
        }

        public void RemoveAddOptions(Author author)
        {
            _authorRepository.SetFields(author, s => s.AddOptions);
        }

        public Author UpdateAuthor(Author author)
        {
            _cache.Clear();
            var storedAuthor = GetAuthor(author.Id);
            var updatedAuthor = _authorRepository.Update(author);
            _eventAggregator.PublishEvent(new AuthorEditedEvent(updatedAuthor, storedAuthor));

            return updatedAuthor;
        }

        public List<Author> UpdateAuthors(List<Author> author, bool useExistingRelativeFolder)
        {
            _cache.Clear();
            _logger.Debug("Updating {0} author", author.Count);

            foreach (var s in author)
            {
                _logger.Trace("Updating: {0}", s.Name);

                if (!s.RootFolderPath.IsNullOrWhiteSpace())
                {
                    s.Path = _authorPathBuilder.BuildPath(s, useExistingRelativeFolder);

                    _logger.Trace("Changing path for {0} to {1}", s.Name, s.Path);
                }
                else
                {
                    _logger.Trace("Not changing path for: {0}", s.Name);
                }
            }

            _authorRepository.UpdateMany(author);
            _logger.Debug("{0} authors updated", author.Count);

            return author;
        }
    }
}
