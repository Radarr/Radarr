using System.Collections.Generic;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;
using Lidarr.Http.Extensions;

namespace Lidarr.Api.V1.Artist
{
    public class ArtistEditorModule : LidarrV1Module
    {
        private readonly IArtistService _artistService;

        public ArtistEditorModule(IArtistService artistService)
            : base("/artist/editor")
        {
            _artistService = artistService;
            Put["/"] = artist => SaveAll();
            Delete["/"] = artist => DeleteArtist();
        }

        private Response SaveAll()
        {
            var resource = Request.Body.FromJson<ArtistEditorResource>();
            var artistToUpdate = _artistService.GetArtists(resource.ArtistIds);

            foreach (var artist in artistToUpdate)
            {
                if (resource.Monitored.HasValue)
                {
                    artist.Monitored = resource.Monitored.Value;
                }

                if (resource.QualityProfileId.HasValue)
                {
                    artist.ProfileId = resource.QualityProfileId.Value;
                }

                if (resource.LanguageProfileId.HasValue)
                {
                    artist.LanguageProfileId = resource.LanguageProfileId.Value;
                }

                if (resource.MetadataProfileId.HasValue)
                {
                    artist.MetadataProfileId = resource.MetadataProfileId.Value;
                }

                if (resource.AlbumFolder.HasValue)
                {
                    artist.AlbumFolder = resource.AlbumFolder.Value;
                }

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    artist.RootFolderPath = resource.RootFolderPath;
                }

                if (resource.Tags != null)
                {
                    var newTags = resource.Tags;
                    var applyTags = resource.ApplyTags;

                    switch (applyTags)
                    {
                        case ApplyTags.Add:
                            newTags.ForEach(t => artist.Tags.Add(t));
                            break;
                        case ApplyTags.Remove:
                            newTags.ForEach(t => artist.Tags.Remove(t));
                            break;
                        case ApplyTags.Replace:
                            artist.Tags = new HashSet<int>(newTags);
                            break;
                    }
                }
            }

            return _artistService.UpdateArtists(artistToUpdate)
                                 .ToResource()
                                 .AsResponse(HttpStatusCode.Accepted);
        }

        private Response DeleteArtist()
        {
            var resource = Request.Body.FromJson<ArtistEditorResource>();

            foreach (var artistId in resource.ArtistIds)
            {
                _artistService.DeleteArtist(artistId, false);
            }

            return new object().AsResponse();
        }
    }
}
