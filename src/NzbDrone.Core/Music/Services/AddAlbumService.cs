using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.MetadataSource;

namespace NzbDrone.Core.Music
{
    public interface IAddAlbumService
    {
        Album AddAlbum(Album album);
        List<Album> AddAlbums(List<Album> albums);
    }

    public class AddAlbumService : IAddAlbumService
    {
        private readonly IArtistService _artistService;
        private readonly IAddArtistService _addArtistService;
        private readonly IAlbumService _albumService;
        private readonly IProvideAlbumInfo _albumInfo;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly Logger _logger;

        public AddAlbumService(IArtistService artistService,
                               IAddArtistService addArtistService,
                               IAlbumService albumService,
                               IProvideAlbumInfo albumInfo,
                               IImportListExclusionService importListExclusionService,
                               Logger logger)
        {
            _artistService = artistService;
            _addArtistService = addArtistService;
            _albumService = albumService;
            _albumInfo = albumInfo;
            _importListExclusionService = importListExclusionService;
            _logger = logger;
        }

        public Album AddAlbum(Album album)
        {
            _logger.Debug($"Adding album {album}");

            album = AddSkyhookData(album);

            // Remove any import list exclusions preventing addition
            _importListExclusionService.Delete(album.ForeignAlbumId);
            _importListExclusionService.Delete(album.ArtistMetadata.Value.ForeignArtistId);

            // Note it's a manual addition so it's not deleted on next refresh
            album.AddOptions.AddType = AlbumAddType.Manual;

            // Add the artist if necessary
            var dbArtist = _artistService.FindById(album.ArtistMetadata.Value.ForeignArtistId);
            if (dbArtist == null)
            {
                var artist = album.Artist.Value;

                artist.Metadata.Value.ForeignArtistId = album.ArtistMetadata.Value.ForeignArtistId;

                dbArtist = _addArtistService.AddArtist(artist, false);
            }

            album.ArtistMetadataId = dbArtist.ArtistMetadataId;
            _albumService.AddAlbum(album);

            return album;
        }

        public List<Album> AddAlbums(List<Album> albums)
        {
            var added = DateTime.UtcNow;
            var addedAlbums = new List<Album>();

            foreach (var a in albums)
            {
                a.Added = added;
                addedAlbums.Add(AddAlbum(a));
            }

            return addedAlbums;
        }

        private Album AddSkyhookData(Album newAlbum)
        {
            Tuple<string, Album, List<ArtistMetadata>> tuple = null;
            try
            {
                tuple = _albumInfo.GetAlbumInfo(newAlbum.ForeignAlbumId);
            }
            catch (AlbumNotFoundException)
            {
                _logger.Error("Album with MusicBrainz Id {0} was not found, it may have been removed from Musicbrainz.", newAlbum.ForeignAlbumId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("MusicbrainzId", "An album with this ID was not found", newAlbum.ForeignAlbumId)
                                              });
            }

            newAlbum.UseMetadataFrom(tuple.Item2);
            newAlbum.Added = DateTime.UtcNow;

            var metadata = tuple.Item3.Single(x => x.ForeignArtistId == tuple.Item1);
            newAlbum.ArtistMetadata = metadata;

            return newAlbum;
        }
    }
}
