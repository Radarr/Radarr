using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Nancy.Responses;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using Nancy.ModelBinding;
using Radarr.Http.Extensions;
using Radarr.Http;
using Radarr.Http.Mapping;

namespace NzbDrone.Api.Config
{
    public class NamingConfigModule : RadarrRestModule<NamingConfigResource>
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly IFilenameSampleService _filenameSampleService;
        private readonly IFilenameValidationService _filenameValidationService;
        private readonly IBuildFileNames _filenameBuilder;

        public NamingConfigModule(INamingConfigService namingConfigService,
                            IFilenameSampleService filenameSampleService,
                            IFilenameValidationService filenameValidationService,
                            IBuildFileNames filenameBuilder)
            : base("config/naming")
        {
            _namingConfigService = namingConfigService;
            _filenameSampleService = filenameSampleService;
            _filenameValidationService = filenameValidationService;
            _filenameBuilder = filenameBuilder;
            GetResourceSingle = GetNamingConfig;
            GetResourceById = GetNamingConfig;
            UpdateResource = UpdateNamingConfig;

            Get["/samples"] = x => GetExamples(this.Bind<NamingConfigResource>());

            SharedValidator.RuleFor(c => c.MultiEpisodeStyle).InclusiveBetween(0, 5);
            SharedValidator.RuleFor(c => c.StandardMovieFormat).ValidMovieFormat();
            SharedValidator.RuleFor(c => c.MovieFolderFormat).ValidMovieFolderFormat();
        }

        private void UpdateNamingConfig(NamingConfigResource resource)
        {
            var nameSpec = resource.ToModel();
            ValidateFormatResult(nameSpec);

            _namingConfigService.Save(nameSpec);
        }

        private NamingConfigResource GetNamingConfig()
        {
            var nameSpec = _namingConfigService.GetConfig();
            var resource = nameSpec.ToResource();

            if (resource.StandardMovieFormat.IsNotNullOrWhiteSpace())
            {
                var basicConfig = _filenameBuilder.GetBasicNamingConfig(nameSpec);
                basicConfig.AddToResource(resource);
            }

            return resource;
        }

        private NamingConfigResource GetNamingConfig(int id)
        {
            return GetNamingConfig();
        }

        private JsonResponse<NamingSampleResource> GetExamples(NamingConfigResource config)
        {
            var nameSpec = config.ToModel();
            var sampleResource = new NamingSampleResource();

            var movieSampleResult = _filenameSampleService.GetMovieSample(nameSpec);

            sampleResource.MovieExample = nameSpec.StandardMovieFormat.IsNullOrWhiteSpace()
                ? "Invalid Format"
                : movieSampleResult.FileName;

            sampleResource.MovieFolderExample = nameSpec.MovieFolderFormat.IsNullOrWhiteSpace()
                ? "Invalid format"
                : _filenameSampleService.GetMovieFolderSample(nameSpec);

            return sampleResource.AsResponse();
        }

        private void ValidateFormatResult(NamingConfig nameSpec)
        {
            var movieSampleResult = _filenameSampleService.GetMovieSample(nameSpec);

            //var standardMovieValidationResult = _filenameValidationService.ValidateMovieFilename(movieSampleResult); For now, let's hope the user is not stupid enough :/

            var validationFailures = new List<ValidationFailure>();

            //validationFailures.AddIfNotNull(standardMovieValidationResult);

            if (validationFailures.Any())
            {
                throw new ValidationException(validationFailures.DistinctBy(v => v.PropertyName).ToArray());
            }
        }
    }
}
