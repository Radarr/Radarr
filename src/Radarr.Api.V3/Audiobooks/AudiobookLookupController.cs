using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Audiobooks;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Audiobooks
{
    [V3ApiController("audiobook/lookup")]
    public class AudiobookLookupController : RestController<AudiobookResource>
    {
        private readonly IAudiobookService _audiobookService;

        public AudiobookLookupController(IAudiobookService audiobookService)
        {
            _audiobookService = audiobookService;
        }

        [NonAction]
        public override ActionResult<AudiobookResource> GetResourceByIdWithErrorHandler(int id)
        {
            throw new NotImplementedException();
        }

        protected override AudiobookResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("isbn")]
        [Produces("application/json")]
        public ActionResult<AudiobookResource> SearchByIsbn(string isbn)
        {
            var audiobook = _audiobookService.FindByIsbn(isbn);
            if (audiobook == null)
            {
                return NotFound();
            }

            return audiobook.ToResource();
        }

        [HttpGet("isbn13")]
        [Produces("application/json")]
        public ActionResult<AudiobookResource> SearchByIsbn13(string isbn13)
        {
            var audiobook = _audiobookService.FindByIsbn13(isbn13);
            if (audiobook == null)
            {
                return NotFound();
            }

            return audiobook.ToResource();
        }

        [HttpGet("asin")]
        [Produces("application/json")]
        public ActionResult<AudiobookResource> SearchByAsin(string asin)
        {
            var audiobook = _audiobookService.FindByAsin(asin);
            if (audiobook == null)
            {
                return NotFound();
            }

            return audiobook.ToResource();
        }

        [HttpGet("foreignid")]
        [Produces("application/json")]
        public ActionResult<AudiobookResource> SearchByForeignId(string foreignId)
        {
            var audiobook = _audiobookService.FindByForeignId(foreignId);
            if (audiobook == null)
            {
                return NotFound();
            }

            return audiobook.ToResource();
        }

        [HttpGet("narrator")]
        [Produces("application/json")]
        public IEnumerable<AudiobookResource> SearchByNarrator(string narrator)
        {
            var audiobooks = _audiobookService.FindByNarrator(narrator);
            return audiobooks.ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public IEnumerable<AudiobookResource> Search([FromQuery] string term)
        {
            // For now, search in local database by title or narrator
            // Future: integrate with metadata provider
            var allAudiobooks = _audiobookService.GetAllAudiobooks();
            var results = new List<AudiobookResource>();

            foreach (var audiobook in allAudiobooks)
            {
                var matchesTitle = audiobook.Title != null &&
                    audiobook.Title.Contains(term, StringComparison.OrdinalIgnoreCase);
                var matchesNarrator = audiobook.Narrator != null &&
                    audiobook.Narrator.Contains(term, StringComparison.OrdinalIgnoreCase);

                if (matchesTitle || matchesNarrator)
                {
                    results.Add(audiobook.ToResource());
                }
            }

            return results;
        }
    }
}
