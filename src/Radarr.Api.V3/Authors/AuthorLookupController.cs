using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Authors;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Authors
{
    [V3ApiController("author/lookup")]
    public class AuthorLookupController : RestController<AuthorResource>
    {
        private readonly IAuthorService _authorService;

        public AuthorLookupController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [NonAction]
        public override ActionResult<AuthorResource> GetResourceByIdWithErrorHandler(int id)
        {
            throw new NotImplementedException();
        }

        protected override AuthorResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("foreignid")]
        [Produces("application/json")]
        public ActionResult<AuthorResource> SearchByForeignId(string foreignId)
        {
            var author = _authorService.FindByForeignId(foreignId);
            if (author == null)
            {
                return NotFound();
            }

            return author.ToResource();
        }

        [HttpGet("name")]
        [Produces("application/json")]
        public ActionResult<AuthorResource> SearchByName(string name)
        {
            var author = _authorService.FindByName(name);
            if (author == null)
            {
                return NotFound();
            }

            return author.ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public IEnumerable<AuthorResource> Search([FromQuery] string term)
        {
            var allAuthors = _authorService.GetAllAuthors();
            var results = new List<AuthorResource>();

            foreach (var author in allAuthors)
            {
                if (author.Name != null &&
                    author.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(author.ToResource());
                }
            }

            return results;
        }
    }
}
