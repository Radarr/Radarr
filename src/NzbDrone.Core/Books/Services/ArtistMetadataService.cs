using System.Collections.Generic;

namespace NzbDrone.Core.Music
{
    public interface IArtistMetadataService
    {
        bool Upsert(AuthorMetadata artist);
        bool UpsertMany(List<AuthorMetadata> artists);
    }

    public class ArtistMetadataService : IArtistMetadataService
    {
        private readonly IArtistMetadataRepository _artistMetadataRepository;

        public ArtistMetadataService(IArtistMetadataRepository artistMetadataRepository)
        {
            _artistMetadataRepository = artistMetadataRepository;
        }

        public bool Upsert(AuthorMetadata artist)
        {
            return _artistMetadataRepository.UpsertMany(new List<AuthorMetadata> { artist });
        }

        public bool UpsertMany(List<AuthorMetadata> artists)
        {
            return _artistMetadataRepository.UpsertMany(artists);
        }
    }
}
