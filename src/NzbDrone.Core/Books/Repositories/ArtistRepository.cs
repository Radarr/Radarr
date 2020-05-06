using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface IArtistRepository : IBasicRepository<Author>
    {
        bool ArtistPathExists(string path);
        Author FindByName(string cleanName);
        Author FindById(string foreignAuthorId);
        Author GetArtistByMetadataId(int artistMetadataId);
        List<Author> GetArtistByMetadataId(IEnumerable<int> artistMetadataId);
    }

    public class ArtistRepository : BasicRepository<Author>, IArtistRepository
    {
        public ArtistRepository(IMainDatabase database,
                                IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override SqlBuilder Builder() => new SqlBuilder()
            .Join<Author, AuthorMetadata>((a, m) => a.AuthorMetadataId == m.Id);

        protected override List<Author> Query(SqlBuilder builder) => Query(_database, builder).ToList();

        public static IEnumerable<Author> Query(IDatabase database, SqlBuilder builder)
        {
            return database.QueryJoined<Author, AuthorMetadata>(builder, (artist, metadata) =>
                    {
                        artist.Metadata = metadata;
                        return artist;
                    });
        }

        public bool ArtistPathExists(string path)
        {
            return Query(c => c.Path == path).Any();
        }

        public Author FindById(string foreignAuthorId)
        {
            return Query(Builder().Where<AuthorMetadata>(m => m.ForeignAuthorId == foreignAuthorId)).SingleOrDefault();
        }

        public Author FindByName(string cleanName)
        {
            cleanName = cleanName.ToLowerInvariant();

            return Query(s => s.CleanName == cleanName).ExclusiveOrDefault();
        }

        public Author GetArtistByMetadataId(int artistMetadataId)
        {
            return Query(s => s.AuthorMetadataId == artistMetadataId).SingleOrDefault();
        }

        public List<Author> GetArtistByMetadataId(IEnumerable<int> artistMetadataIds)
        {
            return Query(s => artistMetadataIds.Contains(s.AuthorMetadataId));
        }
    }
}
