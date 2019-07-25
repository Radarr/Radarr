using NLog;
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
        private readonly Logger _logger;

        public ArtistMetadataService(IArtistMetadataRepository artistMetadataRepository,
                                     Logger logger)
        {
            _artistMetadataRepository = artistMetadataRepository;
            _logger = logger;
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
