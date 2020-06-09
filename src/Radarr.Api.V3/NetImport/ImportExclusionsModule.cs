using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.NetImport.ImportExclusions;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V3.NetImport
{
    public class ImportExclusionsModule : RadarrRestModule<ImportExclusionsResource>
    {
        private readonly IImportExclusionsService _exclusionService;

        public ImportExclusionsModule(NetImportFactory netImportFactory, IImportExclusionsService exclusionService)
            : base("exclusions")
        {
            _exclusionService = exclusionService;
            GetResourceAll = GetAll;
            DeleteResource = RemoveExclusion;
            GetResourceById = GetById;
            Post("/", x => AddExclusions());

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

        public object AddExclusions()
        {
            var resource = Request.Body.FromJson<List<ImportExclusionsResource>>();
            var newMovies = resource.ToModel();

            // TODO: Add some more validation here and auto pull the title if not provided
            return _exclusionService.AddExclusions(newMovies).ToResource();
        }

        public void RemoveExclusion(int id)
        {
            _exclusionService.RemoveExclusion(new ImportExclusion { Id = id });
        }
    }
}
