using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    public interface IEditionRepository : IBasicRepository<Edition>
    {
        Edition FindByForeignEditionId(string foreignEditionId);
        List<Edition> FindByBook(int id);
        List<Edition> FindByAuthor(int id);
        List<Edition> GetEditionsForRefresh(int albumId, IEnumerable<string> foreignEditionIds);
        List<Edition> SetMonitored(Edition edition);
    }

    public class EditionRepository : BasicRepository<Edition>, IEditionRepository
    {
        public EditionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Edition FindByForeignEditionId(string foreignEditionId)
        {
            var edition = Query(x => x.ForeignEditionId == foreignEditionId).SingleOrDefault();

            return edition;
        }

        public List<Edition> GetEditionsForRefresh(int albumId, IEnumerable<string> foreignEditionIds)
        {
            return Query(r => r.BookId == albumId || foreignEditionIds.Contains(r.ForeignEditionId));
        }

        public List<Edition> FindByBook(int id)
        {
            // populate the albums and artist metadata also
            // this hopefully speeds up the track matching a lot
            var builder = new SqlBuilder()
                .LeftJoin<Edition, Book>((e, b) => e.BookId == b.Id)
                .LeftJoin<Book, AuthorMetadata>((b, a) => b.AuthorMetadataId == a.Id)
                .Where<Edition>(r => r.BookId == id);

            return _database.QueryJoined<Edition, Book, AuthorMetadata>(builder, (edition, book, metadata) =>
                    {
                        if (book != null)
                        {
                            book.AuthorMetadata = metadata;
                            edition.Book = book;
                        }

                        return edition;
                    }).ToList();
        }

        public List<Edition> FindByAuthor(int id)
        {
            return Query(Builder().Join<Edition, Book>((e, b) => e.BookId == b.Id)
                         .Join<Book, Author>((b, a) => b.AuthorMetadataId == a.AuthorMetadataId)
                         .Where<Author>(a => a.Id == id));
        }

        public List<Edition> SetMonitored(Edition edition)
        {
            var allEditions = FindByBook(edition.BookId);
            allEditions.ForEach(r => r.Monitored = r.Id == edition.Id);
            Ensure.That(allEditions.Count(x => x.Monitored) == 1).IsTrue();
            UpdateMany(allEditions);
            return allEditions;
        }
    }
}
