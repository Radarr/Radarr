using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.NetImport.ImportExclusions;
using Radarr.Http;

namespace Radarr.Api.V2.NetImport
{
    public class ImportExclusionsModule : RadarrRestModule<ImportExclusionsResource>
    {
        private readonly IImportExclusionsService _exclusionService;

        public ImportExclusionsModule(NetImportFactory netImportFactory, IImportExclusionsService exclusionService) : base("exclusions")
        {
            _exclusionService = exclusionService;
            GetResourceAll = GetAll;
            CreateResource = AddExclusion;
            DeleteResource = RemoveExclusion;
            GetResourceById = GetById;

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
       
        public int AddExclusion(ImportExclusionsResource exclusionResource)
        {
            var model = exclusionResource.ToModel();

            // TODO: Add some more validation here and auto pull the title if not provided

            return _exclusionService.AddExclusion(model).Id;
        }

        public void RemoveExclusion (int id)
        {
            _exclusionService.RemoveExclusion(new ImportExclusion { Id = id });
        }
    }
}
