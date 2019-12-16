using System.Collections.Generic;

namespace NzbDrone.Core.Music
{
    public interface IArtistMetadataService
    {
        bool Upsert(ArtistMetadata artist);
        bool UpsertMany(List<ArtistMetadata> artists);
    }

    public class ArtistMetadataService : IArtistMetadataService
    {
        private readonly IArtistMetadataRepository _artistMetadataRepository;

        public ArtistMetadataService(IArtistMetadataRepository artistMetadataRepository)
        {
            _artistMetadataRepository = artistMetadataRepository;
        }

        public bool Upsert(ArtistMetadata artist)
        {
            return _artistMetadataRepository.UpsertMany(new List<ArtistMetadata> { artist });
        }

        public bool UpsertMany(List<ArtistMetadata> artists)
        {
            return _artistMetadataRepository.UpsertMany(artists);
        }
    }
}
