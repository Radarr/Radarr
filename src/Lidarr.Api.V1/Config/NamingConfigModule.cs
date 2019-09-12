using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Nancy.ModelBinding;
using Nancy.Responses;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using Lidarr.Http;
using Lidarr.Http.Extensions;
using Lidarr.Http.Mapping;

namespace Lidarr.Api.V1.Config
{
    public class NamingConfigModule : LidarrRestModule<NamingConfigResource>
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

            Get("/examples",  x => GetExamples(this.Bind<NamingConfigResource>()));


            SharedValidator.RuleFor(c => c.StandardTrackFormat).ValidTrackFormat();
            SharedValidator.RuleFor(c => c.MultiDiscTrackFormat).ValidTrackFormat();
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

            if (resource.MultiDiscTrackFormat.IsNotNullOrWhiteSpace())
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

        private object GetExamples(NamingConfigResource config)
        {
            if (config.Id == 0)
            {
                config = GetNamingConfig();
            }

            var nameSpec = config.ToModel();
            var sampleResource = new NamingExampleResource();
            
            var singleTrackSampleResult = _filenameSampleService.GetStandardTrackSample(nameSpec);
            var multiDiscTrackSampleResult = _filenameSampleService.GetMultiDiscTrackSample(nameSpec);

            sampleResource.SingleTrackExample = _filenameValidationService.ValidateTrackFilename(singleTrackSampleResult) != null
                    ? null
                    : singleTrackSampleResult.FileName;

            sampleResource.MultiDiscTrackExample = _filenameValidationService.ValidateTrackFilename(multiDiscTrackSampleResult) != null
                    ? null
                    : multiDiscTrackSampleResult.FileName;

            sampleResource.ArtistFolderExample = nameSpec.ArtistFolderFormat.IsNullOrWhiteSpace()
                ? null
                : _filenameSampleService.GetArtistFolderSample(nameSpec);

            sampleResource.AlbumFolderExample = nameSpec.AlbumFolderFormat.IsNullOrWhiteSpace()
                ? null
                : _filenameSampleService.GetAlbumFolderSample(nameSpec);

            return sampleResource;
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
