using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Audiobooks;
using Radarr.Http;

namespace Radarr.Api.V3.Audiobooks
{
    [V3ApiController("audiobook/editor")]
    public class AudiobookEditorController : Controller
    {
        private readonly IAudiobookService _audiobookService;
        private readonly AudiobookEditorValidator _audiobookEditorValidator;

        public AudiobookEditorController(IAudiobookService audiobookService,
                                         AudiobookEditorValidator audiobookEditorValidator)
        {
            _audiobookService = audiobookService;
            _audiobookEditorValidator = audiobookEditorValidator;
        }

        [HttpPut]
        public IActionResult SaveAll([FromBody] AudiobookEditorResource resource)
        {
            var audiobooksToUpdate = _audiobookService.GetAudiobooks(resource.AudiobookIds);

            foreach (var audiobook in audiobooksToUpdate)
            {
                if (resource.Monitored.HasValue)
                {
                    audiobook.Monitored = resource.Monitored.Value;
                }

                if (resource.QualityProfileId.HasValue)
                {
                    audiobook.QualityProfileId = resource.QualityProfileId.Value;
                }

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    audiobook.RootFolderPath = resource.RootFolderPath;
                }

                if (resource.Tags != null)
                {
                    var newTags = resource.Tags;
                    var applyTags = resource.ApplyTags;

                    switch (applyTags)
                    {
                        case ApplyTags.Add:
                            newTags.ForEach(t => audiobook.Tags.Add(t));
                            break;
                        case ApplyTags.Remove:
                            newTags.ForEach(t => audiobook.Tags.Remove(t));
                            break;
                        case ApplyTags.Replace:
                            audiobook.Tags = new HashSet<int>(newTags);
                            break;
                    }
                }

                var validationResult = _audiobookEditorValidator.Validate(audiobook);

                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }

            var updatedAudiobooks = _audiobookService.UpdateAudiobooks(audiobooksToUpdate);

            var audiobooksResources = new List<AudiobookResource>(updatedAudiobooks.Count);

            foreach (var audiobook in updatedAudiobooks)
            {
                audiobooksResources.Add(audiobook.ToResource());
            }

            return Ok(audiobooksResources);
        }

        [HttpDelete]
        public object DeleteAudiobooks([FromBody] AudiobookEditorResource resource)
        {
            _audiobookService.DeleteAudiobooks(resource.AudiobookIds, resource.DeleteFiles);

            return new { };
        }
    }
}
