using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.CustomFormats;
using Radarr.Http;

namespace NzbDrone.Api.CustomFormats
{
    public class CustomFormatModule : RadarrRestModule<CustomFormatResource>
    {
        private readonly ICustomFormatService _formatService;

        public CustomFormatModule(ICustomFormatService formatService)
        {
            _formatService = formatService;

            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Name)
                .Must((v, c) => !_formatService.All().Any(f => f.Name == c && f.Id != v.Id)).WithMessage("Must be unique.");
            SharedValidator.RuleFor(c => c.Specifications).NotEmpty();

            GetResourceAll = GetAll;

            GetResourceById = GetById;

            UpdateResource = Update;

            CreateResource = Create;

            DeleteResource = DeleteFormat;

            Get("schema", x => GetTemplates());
        }

        private int Create(CustomFormatResource customFormatResource)
        {
            var model = customFormatResource.ToModel();
            return _formatService.Insert(model).Id;
        }

        private void Update(CustomFormatResource resource)
        {
            var model = resource.ToModel();
            _formatService.Update(model);
        }

        private CustomFormatResource GetById(int id)
        {
            return _formatService.GetById(id).ToResource();
        }

        private List<CustomFormatResource> GetAll()
        {
            return _formatService.All().ToResource();
        }

        private void DeleteFormat(int id)
        {
            _formatService.Delete(id);
        }

        private object GetTemplates()
        {
            return null;
        }
    }
}
