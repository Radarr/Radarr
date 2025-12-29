using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.Music;
using Radarr.Api.V3.MediaItems;
using Radarr.Http;

namespace Radarr.Api.V3.Music
{
    [V3ApiController("album/editor")]
    public class AlbumEditorController : BaseMediaEditorController<Album, AlbumResource, AlbumEditorResource>
    {
        private readonly IAlbumService _albumService;
        private readonly AlbumEditorValidator _albumEditorValidator;

        public AlbumEditorController(IAlbumService albumService, AlbumEditorValidator albumEditorValidator)
        {
            _albumService = albumService;
            _albumEditorValidator = albumEditorValidator;
        }

        protected override List<Album> GetItemsByIds(List<int> ids) => _albumService.GetAlbums(ids);
        protected override List<Album> UpdateItems(List<Album> items) => _albumService.UpdateAlbums(items);
        protected override void DeleteItems(List<int> ids, bool deleteFiles) => _albumService.DeleteAlbums(ids, deleteFiles);
        protected override ValidationResult ValidateItem(Album item) => _albumEditorValidator.Validate(item);
        protected override AlbumResource ToResource(Album model) => model.ToResource();

        protected override bool GetMonitored(Album item) => item.Monitored;
        protected override void SetMonitored(Album item, bool monitored) => item.Monitored = monitored;
        protected override int GetQualityProfileId(Album item) => item.QualityProfileId;
        protected override void SetQualityProfileId(Album item, int qualityProfileId) => item.QualityProfileId = qualityProfileId;
        protected override string GetRootFolderPath(Album item) => item.RootFolderPath;
        protected override void SetRootFolderPath(Album item, string rootFolderPath) => item.RootFolderPath = rootFolderPath;
        protected override HashSet<int> GetTags(Album item) => item.Tags;
        protected override void SetTags(Album item, HashSet<int> tags) => item.Tags = tags;
    }
}
