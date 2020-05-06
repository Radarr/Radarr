using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Music
{
    public interface IArtistService
    {
        Author GetArtist(int authorId);
        Author GetArtistByMetadataId(int artistMetadataId);
        List<Author> GetArtists(IEnumerable<int> authorIds);
        Author AddArtist(Author newArtist, bool doRefresh);
        List<Author> AddArtists(List<Author> newArtists, bool doRefresh);
        Author FindById(string foreignAuthorId);
        Author FindByName(string title);
        Author FindByNameInexact(string title);
        List<Author> GetCandidates(string title);
        List<Author> GetReportCandidates(string reportTitle);
        void DeleteArtist(int authorId, bool deleteFiles, bool addImportListExclusion = false);
        List<Author> GetAllArtists();
        List<Author> AllForTag(int tagId);
        Author UpdateArtist(Author artist);
        List<Author> UpdateArtists(List<Author> artist, bool useExistingRelativeFolder);
        bool ArtistPathExists(string folder);
        void RemoveAddOptions(Author artist);
    }

    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository _artistRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildArtistPaths _artistPathBuilder;
        private readonly Logger _logger;
        private readonly ICached<List<Author>> _cache;

        public ArtistService(IArtistRepository artistRepository,
                             IEventAggregator eventAggregator,
                             IBuildArtistPaths artistPathBuilder,
                             ICacheManager cacheManager,
                             Logger logger)
        {
            _artistRepository = artistRepository;
            _eventAggregator = eventAggregator;
            _artistPathBuilder = artistPathBuilder;
            _cache = cacheManager.GetCache<List<Author>>(GetType());
            _logger = logger;
        }

        public Author AddArtist(Author newArtist, bool doRefresh)
        {
            _cache.Clear();
            _artistRepository.Insert(newArtist);
            _eventAggregator.PublishEvent(new ArtistAddedEvent(GetArtist(newArtist.Id), doRefresh));

            return newArtist;
        }

        public List<Author> AddArtists(List<Author> newArtists, bool doRefresh)
        {
            _cache.Clear();
            _artistRepository.InsertMany(newArtists);
            _eventAggregator.PublishEvent(new ArtistsImportedEvent(newArtists.Select(s => s.Id).ToList(), doRefresh));

            return newArtists;
        }

        public bool ArtistPathExists(string folder)
        {
            return _artistRepository.ArtistPathExists(folder);
        }

        public void DeleteArtist(int authorId, bool deleteFiles, bool addImportListExclusion = false)
        {
            _cache.Clear();
            var artist = _artistRepository.Get(authorId);
            _artistRepository.Delete(authorId);
            _eventAggregator.PublishEvent(new ArtistDeletedEvent(artist, deleteFiles, addImportListExclusion));
        }

        public Author FindById(string foreignAuthorId)
        {
            return _artistRepository.FindById(foreignAuthorId);
        }

        public Author FindByName(string title)
        {
            return _artistRepository.FindByName(title.CleanArtistName());
        }

        public List<Tuple<Func<Author, string, double>, string>> ArtistScoringFunctions(string title, string cleanTitle)
        {
            Func<Func<Author, string, double>, string, Tuple<Func<Author, string, double>, string>> tc = Tuple.Create;
            var scoringFunctions = new List<Tuple<Func<Author, string, double>, string>>
            {
                tc((a, t) => a.CleanName.FuzzyMatch(t), cleanTitle),
                tc((a, t) => a.Name.FuzzyMatch(t), title),
                tc((a, t) => a.Metadata.Value.Aliases.Concat(new List<string> { a.Name }).Max(x => x.CleanArtistName().FuzzyMatch(t)), cleanTitle),
            };

            if (title.StartsWith("The ", StringComparison.CurrentCultureIgnoreCase))
            {
                scoringFunctions.Add(tc((a, t) => a.CleanName.FuzzyMatch(t), title.Substring(4).CleanArtistName()));
            }
            else
            {
                scoringFunctions.Add(tc((a, t) => a.CleanName.FuzzyMatch(t), "the" + cleanTitle));
            }

            return scoringFunctions;
        }

        public Author FindByNameInexact(string title)
        {
            var artists = GetAllArtists();

            foreach (var func in ArtistScoringFunctions(title, title.CleanArtistName()))
            {
                var results = FindByStringInexact(artists, func.Item1, func.Item2);
                if (results.Count == 1)
                {
                    return results[0];
                }
            }

            return null;
        }

        public List<Author> GetCandidates(string title)
        {
            var artists = GetAllArtists();
            var output = new List<Author>();

            foreach (var func in ArtistScoringFunctions(title, title.CleanArtistName()))
            {
                output.AddRange(FindByStringInexact(artists, func.Item1, func.Item2));
            }

            return output.DistinctBy(x => x.Id).ToList();
        }

        public List<Tuple<Func<Author, string, double>, string>> ReportArtistScoringFunctions(string reportTitle, string cleanReportTitle)
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
            var artists = GetAllArtists();
            var output = new List<Author>();

            foreach (var func in ArtistScoringFunctions(reportTitle, reportTitle.CleanArtistName()))
            {
                output.AddRange(FindByStringInexact(artists, func.Item1, func.Item2));
            }

            return output.DistinctBy(x => x.Id).ToList();
        }

        private List<Author> FindByStringInexact(List<Author> artists, Func<Author, string, double> scoreFunction, string title)
        {
            const double fuzzThreshold = 0.8;
            const double fuzzGap = 0.2;

            var sortedArtists = artists.Select(s => new
            {
                MatchProb = scoreFunction(s, title),
                Artist = s
            })
                .ToList()
                .OrderByDescending(s => s.MatchProb)
                .ToList();

            return sortedArtists.TakeWhile((x, i) => i == 0 || sortedArtists[i - 1].MatchProb - x.MatchProb < fuzzGap)
                .TakeWhile((x, i) => x.MatchProb > fuzzThreshold || (i > 0 && sortedArtists[i - 1].MatchProb > fuzzThreshold))
                .Select(x => x.Artist)
                .ToList();
        }

        public List<Author> GetAllArtists()
        {
            return _cache.Get("GetAllArtists", () => _artistRepository.All().ToList(), TimeSpan.FromSeconds(30));
        }

        public List<Author> AllForTag(int tagId)
        {
            return GetAllArtists().Where(s => s.Tags.Contains(tagId))
                                 .ToList();
        }

        public Author GetArtist(int authorId)
        {
            return _artistRepository.Get(authorId);
        }

        public Author GetArtistByMetadataId(int artistMetadataId)
        {
            return _artistRepository.GetArtistByMetadataId(artistMetadataId);
        }

        public List<Author> GetArtists(IEnumerable<int> authorIds)
        {
            return _artistRepository.Get(authorIds).ToList();
        }

        public void RemoveAddOptions(Author artist)
        {
            _artistRepository.SetFields(artist, s => s.AddOptions);
        }

        public Author UpdateArtist(Author artist)
        {
            _cache.Clear();
            var storedArtist = GetArtist(artist.Id);
            var updatedArtist = _artistRepository.Update(artist);
            _eventAggregator.PublishEvent(new ArtistEditedEvent(updatedArtist, storedArtist));

            return updatedArtist;
        }

        public List<Author> UpdateArtists(List<Author> artist, bool useExistingRelativeFolder)
        {
            _cache.Clear();
            _logger.Debug("Updating {0} artist", artist.Count);

            foreach (var s in artist)
            {
                _logger.Trace("Updating: {0}", s.Name);

                if (!s.RootFolderPath.IsNullOrWhiteSpace())
                {
                    s.Path = _artistPathBuilder.BuildPath(s, useExistingRelativeFolder);

                    _logger.Trace("Changing path for {0} to {1}", s.Name, s.Path);
                }
                else
                {
                    _logger.Trace("Not changing path for: {0}", s.Name);
                }
            }

            _artistRepository.UpdateMany(artist);
            _logger.Debug("{0} artists updated", artist.Count);

            return artist;
        }
    }
}
