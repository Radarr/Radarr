using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using Marr.Data.QGen;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Music
{
    public interface IArtistRepository : IBasicRepository<Artist>
    {
        bool ArtistPathExists(string path);
        Artist FindByName(string cleanTitle);
        Artist FindById(string foreignArtistId);
        Artist GetArtistByMetadataId(int artistMetadataId);
    }

    public class ArtistRepository : BasicRepository<Artist>, IArtistRepository
    {
        public ArtistRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        // Always explicitly join with ArtistMetadata to populate Metadata without repeated LazyLoading
        protected override QueryBuilder<Artist> Query => DataMapper.Query<Artist>().Join<Artist, ArtistMetadata>(JoinType.Inner, a => a.Metadata, (l, r) => l.ArtistMetadataId == r.Id);

        public bool ArtistPathExists(string path)
        {
            return Query.Where(c => c.Path == path).Any();
        }

        public Artist FindById(string foreignArtistId)
        {
            return Query.Where<ArtistMetadata>(m => m.ForeignArtistId == foreignArtistId).SingleOrDefault();
        }

        public Artist FindByName(string cleanName)
        {
            cleanName = cleanName.ToLowerInvariant();

            return Query.Where(s => s.CleanName == cleanName).ExclusiveOrDefault();
        }

        public Artist GetArtistByMetadataId(int artistMetadataId)
        {
            return Query.Where(s => s.ArtistMetadataId == artistMetadataId).SingleOrDefault();
        }
    }
}
