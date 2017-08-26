using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Nancy.Responses;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using Nancy.ModelBinding;
using NzbDrone.Api.Extensions;

namespace NzbDrone.Api.Config
{
    public class NamingConfigModule : NzbDroneRestModule<NamingConfigResource>
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
            SharedValidator.RuleFor(c => c.StandardTrackFormat).ValidTrackFormat();
            SharedValidator.RuleFor(c => c.ArtistFolderFormat).ValidArtistFolderFormat();
            SharedValidator.RuleFor(c => c.AlbumFolderFormat).ValidAlbumFolderFormat();
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

            if (resource.StandardTrackFormat.IsNotNullOrWhiteSpace())
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
            

            var singleTrackSampleResult = _filenameSampleService.GetStandardTrackSample(nameSpec);

            sampleResource.SingleTrackExample = _filenameValidationService.ValidateTrackFilename(singleTrackSampleResult) != null
                    ? "Invalid format"
                    : singleTrackSampleResult.FileName;

            sampleResource.ArtistFolderExample = nameSpec.ArtistFolderFormat.IsNullOrWhiteSpace()
                ? "Invalid format"
                : _filenameSampleService.GetArtistFolderSample(nameSpec);

            sampleResource.AlbumFolderExample = nameSpec.AlbumFolderFormat.IsNullOrWhiteSpace()
                ? "Invalid format"
                : _filenameSampleService.GetAlbumFolderSample(nameSpec);

            return sampleResource.AsResponse();
        }

        private void ValidateFormatResult(NamingConfig nameSpec)
        {

            var singleTrackSampleResult = _filenameSampleService.GetStandardTrackSample(nameSpec);
            
            var singleTrackValidationResult = _filenameValidationService.ValidateTrackFilename(singleTrackSampleResult);

            var validationFailures = new List<ValidationFailure>();

            validationFailures.AddIfNotNull(singleTrackValidationResult);


            if (validationFailures.Any())
            {
                throw new ValidationException(validationFailures.DistinctBy(v => v.PropertyName).ToArray());
            }
        }
    }
}
