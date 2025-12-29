using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Audiobooks
{
    public interface IAudiobookRepository : IBasicRepository<Audiobook>
    {
        bool AudiobookPathExists(string path);
        Audiobook FindByIsbn(string isbn);
        Audiobook FindByIsbn13(string isbn13);
        Audiobook FindByAsin(string asin);
        Audiobook FindByForeignId(string foreignAudiobookId);
        List<Audiobook> FindByAuthorId(int authorId);
        List<Audiobook> FindBySeriesId(int seriesId);
        List<Audiobook> FindByBookId(int bookId);
        List<Audiobook> FindByNarrator(string narrator);
        List<Audiobook> AudiobooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        Audiobook FindByPath(string path);
        Dictionary<int, string> AllAudiobookPaths();
        Dictionary<int, List<int>> AllAudiobookTags();
    }

    public class AudiobookRepository : BasicRepository<Audiobook>, IAudiobookRepository
    {
        public AudiobookRepository(IMainDatabase database,
                                   IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool AudiobookPathExists(string path)
        {
            return Query(a => a.Path == path).Any();
        }

        public Audiobook FindByIsbn(string isbn)
        {
            return Query(a => a.Isbn == isbn).FirstOrDefault();
        }

        public Audiobook FindByIsbn13(string isbn13)
        {
            return Query(a => a.Isbn13 == isbn13).FirstOrDefault();
        }

        public Audiobook FindByAsin(string asin)
        {
            return Query(a => a.Asin == asin).FirstOrDefault();
        }

        public Audiobook FindByForeignId(string foreignAudiobookId)
        {
            return Query(a => a.ForeignAudiobookId == foreignAudiobookId).FirstOrDefault();
        }

        public List<Audiobook> FindByAuthorId(int authorId)
        {
            return Query(a => a.AuthorId == authorId);
        }

        public List<Audiobook> FindBySeriesId(int seriesId)
        {
            return Query(a => a.SeriesId == seriesId);
        }

        public List<Audiobook> FindByBookId(int bookId)
        {
            return Query(a => a.BookId == bookId);
        }

        public List<Audiobook> FindByNarrator(string narrator)
        {
            return Query(a => a.Narrator == narrator);
        }

        public List<Audiobook> AudiobooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var query = Query(a => a.ReleaseDate >= start && a.ReleaseDate <= end);

            if (!includeUnmonitored)
            {
                query = query.Where(a => a.Monitored).ToList();
            }

            return query;
        }

        public Audiobook FindByPath(string path)
        {
            return Query(a => a.Path == path).FirstOrDefault();
        }

        public Dictionary<int, string> AllAudiobookPaths()
        {
            var audiobooks = All();
            return audiobooks.ToDictionary(a => a.Id, a => a.Path);
        }

        public Dictionary<int, List<int>> AllAudiobookTags()
        {
            var audiobooks = All();
            return audiobooks.ToDictionary(a => a.Id, a => a.Tags.ToList());
        }
    }
}
