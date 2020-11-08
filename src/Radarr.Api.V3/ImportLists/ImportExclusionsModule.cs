using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.ImportLists.ImportExclusions;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V3.ImportLists
{
    public class ImportExclusionsModule : RadarrRestModule<ImportExclusionsResource>
    {
        private readonly IImportExclusionsService _exclusionService;

        public ImportExclusionsModule(IImportExclusionsService exclusionService)
            : base("exclusions")
        {
            _exclusionService = exclusionService;

            GetResourceAll = GetAll;
            DeleteResource = RemoveExclusion;
            CreateResource = AddExclusion;
            GetResourceById = GetById;
            UpdateResource = UpdateExclusion;
            Post("/bulk", x => AddExclusions());

            SharedValidator.RuleFor(c => c.TmdbId).GreaterThan(0);
            SharedValidator.RuleFor(c => c.MovieTitle).NotEmpty();
            SharedValidator.RuleFor(c => c.MovieYear).GreaterThan(0);
        }

        public List<ImportExclusionsResource> GetAll()
        {
            return _exclusionService.GetAllExclusions().ToResource();
        }

        public ImportExclusionsResource GetById(int id)
        {
            return _exclusionService.GetById(id).ToResource();
        }

        private void UpdateExclusion(ImportExclusionsResource exclusionResource)
        {
            var model = exclusionResource.ToModel();
            _exclusionService.Update(model);
        }

        public int AddExclusion(ImportExclusionsResource exclusionResource)
        {
            var model = exclusionResource.ToModel();

            return _exclusionService.AddExclusion(model).Id;
        }

        public object AddExclusions()
        {
            var resource = Request.Body.FromJson<List<ImportExclusionsResource>>();
            var newMovies = resource.ToModel();

            return _exclusionService.AddExclusions(newMovies).ToResource();
        }

        public void RemoveExclusion(int id)
        {
            _exclusionService.RemoveExclusion(new ImportExclusion { Id = id });
        }
    }
}
