using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.Audiobooks;
using Radarr.Api.V3.MediaItems;
using Radarr.Http;

namespace Radarr.Api.V3.Audiobooks
{
    [V3ApiController("audiobook/editor")]
    public class AudiobookEditorController : BaseMediaEditorController<Audiobook, AudiobookResource, AudiobookEditorResource>
    {
        private readonly IAudiobookService _audiobookService;
        private readonly AudiobookEditorValidator _audiobookEditorValidator;

        public AudiobookEditorController(IAudiobookService audiobookService, AudiobookEditorValidator audiobookEditorValidator)
        {
            _audiobookService = audiobookService;
            _audiobookEditorValidator = audiobookEditorValidator;
        }

        protected override List<Audiobook> GetItemsByIds(List<int> ids) => _audiobookService.GetAudiobooks(ids);
        protected override List<Audiobook> UpdateItems(List<Audiobook> items) => _audiobookService.UpdateAudiobooks(items);
        protected override void DeleteItems(List<int> ids, bool deleteFiles) => _audiobookService.DeleteAudiobooks(ids, deleteFiles);
        protected override ValidationResult ValidateItem(Audiobook item) => _audiobookEditorValidator.Validate(item);
        protected override AudiobookResource ToResource(Audiobook model) => model.ToResource();

        protected override bool GetMonitored(Audiobook item) => item.Monitored;
        protected override void SetMonitored(Audiobook item, bool monitored) => item.Monitored = monitored;
        protected override int GetQualityProfileId(Audiobook item) => item.QualityProfileId;
        protected override void SetQualityProfileId(Audiobook item, int qualityProfileId) => item.QualityProfileId = qualityProfileId;
        protected override string GetRootFolderPath(Audiobook item) => item.RootFolderPath;
        protected override void SetRootFolderPath(Audiobook item, string rootFolderPath) => item.RootFolderPath = rootFolderPath;
        protected override HashSet<int> GetTags(Audiobook item) => item.Tags;
        protected override void SetTags(Audiobook item, HashSet<int> tags) => item.Tags = tags;
    }
}
