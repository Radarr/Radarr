using System.Collections.Generic;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.ImportExclusions;
using Radarr.Http;

namespace NzbDrone.Api.ImportList
{
    public class ImportExclusionsModule : RadarrRestModule<ImportExclusionsResource>
    {
        private readonly IImportExclusionsService _exclusionService;

        public ImportExclusionsModule(ImportListFactory importListFactory, IImportExclusionsService exclusionService)
            : base("exclusions")
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

        public void RemoveExclusion(int id)
        {
            _exclusionService.RemoveExclusion(new ImportExclusion { Id = id });
        }
    }
}
