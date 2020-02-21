using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface IArtistRepository : IBasicRepository<Artist>
    {
        bool ArtistPathExists(string path);
        Artist FindByName(string cleanName);
        Artist FindById(string foreignArtistId);
        Artist GetArtistByMetadataId(int artistMetadataId);
        List<Artist> GetArtistByMetadataId(IEnumerable<int> artistMetadataId);
    }

    public class ArtistRepository : BasicRepository<Artist>, IArtistRepository
    {
        public ArtistRepository(IMainDatabase database,
                                IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override SqlBuilder Builder() => new SqlBuilder()
            .Join<Artist, ArtistMetadata>((a, m) => a.ArtistMetadataId == m.Id);

        protected override List<Artist> Query(SqlBuilder builder) => Query(_database, builder).ToList();

        public static IEnumerable<Artist> Query(IDatabase database, SqlBuilder builder)
        {
            return database.QueryJoined<Artist, ArtistMetadata>(builder, (artist, metadata) =>
                    {
                        artist.Metadata = metadata;
                        return artist;
                    });
        }

        public bool ArtistPathExists(string path)
        {
            return Query(c => c.Path == path).Any();
        }

        public Artist FindById(string foreignArtistId)
        {
            var artist = Query(Builder().Where<ArtistMetadata>(m => m.ForeignArtistId == foreignArtistId)).SingleOrDefault();

            if (artist == null)
            {
                artist = Query(Builder().Where<ArtistMetadata>(x => x.OldForeignArtistIds.Contains(foreignArtistId))).SingleOrDefault();
            }

            return artist;
        }

        public Artist FindByName(string cleanName)
        {
            cleanName = cleanName.ToLowerInvariant();

            return Query(s => s.CleanName == cleanName).ExclusiveOrDefault();
        }

        public Artist GetArtistByMetadataId(int artistMetadataId)
        {
            return Query(s => s.ArtistMetadataId == artistMetadataId).SingleOrDefault();
        }

        public List<Artist> GetArtistByMetadataId(IEnumerable<int> artistMetadataIds)
        {
            return Query(s => artistMetadataIds.Contains(s.ArtistMetadataId));
        }
    }
}
