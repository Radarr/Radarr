using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.Music;
using Radarr.Api.V3.MediaItems;
using Radarr.Http;

namespace Radarr.Api.V3.Music
{
    [V3ApiController("artist/editor")]
    public class ArtistEditorController : BaseMediaEditorController<Artist, ArtistResource, ArtistEditorResource>
    {
        private readonly IArtistService _artistService;
        private readonly ArtistEditorValidator _artistEditorValidator;

        public ArtistEditorController(IArtistService artistService, ArtistEditorValidator artistEditorValidator)
        {
            _artistService = artistService;
            _artistEditorValidator = artistEditorValidator;
        }

        protected override List<Artist> GetItemsByIds(List<int> ids) => _artistService.GetArtists(ids);
        protected override List<Artist> UpdateItems(List<Artist> items) => _artistService.UpdateArtists(items);
        protected override void DeleteItems(List<int> ids, bool deleteFiles) => _artistService.DeleteArtists(ids, deleteFiles);
        protected override ValidationResult ValidateItem(Artist item) => _artistEditorValidator.Validate(item);
        protected override ArtistResource ToResource(Artist model) => model.ToResource();

        protected override bool GetMonitored(Artist item) => item.Monitored;
        protected override void SetMonitored(Artist item, bool monitored) => item.Monitored = monitored;
        protected override int GetQualityProfileId(Artist item) => item.QualityProfileId;
        protected override void SetQualityProfileId(Artist item, int qualityProfileId) => item.QualityProfileId = qualityProfileId;
        protected override string GetRootFolderPath(Artist item) => item.RootFolderPath;
        protected override void SetRootFolderPath(Artist item, string rootFolderPath) => item.RootFolderPath = rootFolderPath;
        protected override HashSet<int> GetTags(Artist item) => item.Tags;
        protected override void SetTags(Artist item, HashSet<int> tags) => item.Tags = tags;
    }
}
