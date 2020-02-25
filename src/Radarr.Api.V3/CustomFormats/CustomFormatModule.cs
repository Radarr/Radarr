using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.CustomFormats;
using Radarr.Http;

namespace Radarr.Api.V3.CustomFormats
{
    public class CustomFormatModule : RadarrRestModule<CustomFormatResource>
    {
        private readonly ICustomFormatService _formatService;
        private readonly List<ICustomFormatSpecification> _specifications;

        public CustomFormatModule(ICustomFormatService formatService,
                                  List<ICustomFormatSpecification> specifications)
        {
            _formatService = formatService;
            _specifications = specifications;

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
            var model = customFormatResource.ToModel(_specifications);
            return _formatService.Insert(model).Id;
        }

        private void Update(CustomFormatResource resource)
        {
            var model = resource.ToModel(_specifications);
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
            var schema = _specifications.OrderBy(x => x.Order).Select(x => x.ToSchema()).ToList();

            var presets = GetPresets();

            foreach (var item in schema)
            {
                item.Presets = presets.Where(x => x.GetType().Name == item.Implementation).Select(x => x.ToSchema()).ToList();
            }

            return schema;
        }

        private IEnumerable<ICustomFormatSpecification> GetPresets()
        {
            yield return new ReleaseTitleSpecification
            {
                Name = "x264",
                Value = @"(x|h)\.?264"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "x265",
                Value = @"(((x|h)\.?265)|(HEVC))"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "Simple Hardcoded Subs",
                Value = @"subs?"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "Hardcoded Subs",
                Value = @"\b(?<hcsub>(\w+SUBS?)\b)|(?<hc>(HC|SUBBED))\b"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "Surround Sound",
                Value = @"DTS.?(HD|ES|X(?!\D))|TRUEHD|ATMOS|DD(\+|P).?([5-9])|EAC3.?([5-9])"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "Preferred Words",
                Value = @"\b(SPARKS|Framestor)\b"
            };

            var formats = _formatService.All();
            foreach (var format in formats)
            {
                foreach (var condition in format.Specifications)
                {
                    var preset = condition.Clone();
                    preset.Name = $"{format.Name}: {preset.Name}";
                    yield return preset;
                }
            }
        }
    }
}
