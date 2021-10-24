using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.ImportLists.ImportExclusions;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.ImportLists
{
    [V3ApiController("exclusions")]
    public class ImportExclusionsController : RestController<ImportExclusionsResource>
    {
        private readonly IImportExclusionsService _exclusionService;

        public ImportExclusionsController(IImportExclusionsService exclusionService)
        {
            _exclusionService = exclusionService;

            SharedValidator.RuleFor(c => c.TmdbId).GreaterThan(0);
            SharedValidator.RuleFor(c => c.MovieTitle).NotEmpty();
            SharedValidator.RuleFor(c => c.MovieYear).GreaterThan(0);
        }

        [HttpGet]
        public List<ImportExclusionsResource> GetAll()
        {
            return _exclusionService.GetAllExclusions().ToResource();
        }

        protected override ImportExclusionsResource GetResourceById(int id)
        {
            return _exclusionService.GetById(id).ToResource();
        }

        [RestPutById]
        public ActionResult<ImportExclusionsResource> UpdateExclusion(ImportExclusionsResource exclusionResource)
        {
            var model = exclusionResource.ToModel();
            return Accepted(_exclusionService.Update(model));
        }

        [RestPostById]
        public ActionResult<ImportExclusionsResource> AddExclusion(ImportExclusionsResource exclusionResource)
        {
            var model = exclusionResource.ToModel();

            return Created(_exclusionService.AddExclusion(model).Id);
        }

        [HttpPost("bulk")]
        public object AddExclusions([FromBody] List<ImportExclusionsResource> resource)
        {
            var newMovies = resource.ToModel();

            return _exclusionService.AddExclusions(newMovies).ToResource();
        }

        [RestDeleteById]
        public void RemoveExclusion(int id)
        {
            _exclusionService.RemoveExclusion(new ImportExclusion { Id = id });
        }
    }
}
