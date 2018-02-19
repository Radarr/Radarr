using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.NetImport.ImportExclusions;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Api.NetImport
{
    public class ImportExclusionsModule : NzbDroneRestModule<ImportExclusionsResource>
    {
        private readonly IImportExclusionsService _exclusionService;

        public ImportExclusionsModule(NetImportFactory netImportFactory, IImportExclusionsService exclusionService) : base("exclusions")
        {
            _exclusionService = exclusionService;
            GetResourceAll = GetAll;
            CreateResource = AddExclusion;
            DeleteResource = RemoveExclusion;
            GetResourceById = GetById;
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

            return _exclusionService.AddExclusion(model).Id;
        }

        public void RemoveExclusion (int id)
        {
            _exclusionService.RemoveExclusion(new ImportExclusion { Id = id });
        }
    }
}
