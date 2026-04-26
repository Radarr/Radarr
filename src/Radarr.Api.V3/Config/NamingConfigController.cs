using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Config
{
    [V3ApiController("config/naming")]
    public class NamingConfigController : RestController<NamingConfigResource>
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly IFilenameSampleService _filenameSampleService;
        private readonly IFilenameValidationService _filenameValidationService;

        public NamingConfigController(INamingConfigService namingConfigService,
                                  IFilenameSampleService filenameSampleService,
                                  IFilenameValidationService filenameValidationService)
        {
            _namingConfigService = namingConfigService;
            _filenameSampleService = filenameSampleService;
            _filenameValidationService = filenameValidationService;

            SharedValidator.RuleFor(c => c.StandardMovieFormat).ValidMovieFormat();
            SharedValidator.RuleFor(c => c.MovieFolderFormat).ValidMovieFolderFormat();
        }

        protected override NamingConfigResource GetResourceById(int id)
        {
            return _namingConfigService.GetConfig().ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<NamingConfigResource> GetNamingConfig()
        {
            return TypedResults.Ok(GetResourceById(1));
        }

        [RestPutById]
        [Consumes("application/json")]
        public Results<Accepted<NamingConfigResource>, NotFound> UpdateNamingConfig([FromBody] NamingConfigResource resource)
        {
            var nameSpec = resource.ToModel();
            ValidateFormatResult(nameSpec);

            _namingConfigService.Save(nameSpec);

            return Accepted(resource.Id);
        }

        [HttpGet("examples")]
        [Produces("application/json")]
        public Ok<NamingExampleResource> GetExamples([FromQuery] NamingConfigResource config)
        {
            if (config.Id == 0)
            {
                config = GetResourceById(1);
            }

            var nameSpec = config.ToModel();
            var sampleResource = new NamingExampleResource();

            var movieSampleResult = _filenameSampleService.GetMovieSample(nameSpec);

            sampleResource.MovieExample = nameSpec.StandardMovieFormat.IsNullOrWhiteSpace()
                ? null
                : movieSampleResult.FileName;

            sampleResource.MovieFolderExample = nameSpec.MovieFolderFormat.IsNullOrWhiteSpace()
                ? null
                : _filenameSampleService.GetMovieFolderSample(nameSpec);

            return TypedResults.Ok(sampleResource);
        }

        private void ValidateFormatResult(NamingConfig nameSpec)
        {
            var movieSampleResult = _filenameSampleService.GetMovieSample(nameSpec);

            var standardMovieValidationResult = _filenameValidationService.ValidateMovieFilename(movieSampleResult);

            var validationFailures = new List<ValidationFailure>();

            validationFailures.AddIfNotNull(standardMovieValidationResult);

            if (validationFailures.Any())
            {
                throw new ValidationException(validationFailures.DistinctBy(v => v.PropertyName).ToArray());
            }
        }
    }
}
