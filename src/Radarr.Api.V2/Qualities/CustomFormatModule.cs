using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Nancy;
using Radarr.Http.Extensions;
using Radarr.Http.Validation;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Parser;
using Radarr.Http;

namespace Radarr.Api.V2.Qualities
{
    public class CustomFormatModule : RadarrRestModule<CustomFormatResource>
    {
        private readonly ICustomFormatService _formatService;
        private readonly IParsingService _parsingService;

        public CustomFormatModule(ICustomFormatService formatService, IParsingService parsingService)
        {
            _formatService = formatService;
            _parsingService = parsingService;

            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Name)
                .Must((v, c) => !_formatService.All().Any(f => f.Name == c && f.Id != v.Id)).WithMessage("Must be unique.");
            SharedValidator.RuleFor(c => c.FormatTags).SetValidator(new FormatTagValidator());
            SharedValidator.RuleFor(c => c.FormatTags).Must((v, c) =>
                {
                    var allFormats = _formatService.All();
                    return !allFormats.Any(f =>
                    {
                        var allTags = f.FormatTags.Select(t => t.Raw.ToLower());
                        var allNewTags = c.Select(t => t.ToLower());
                        var enumerable = allTags.ToList();
                        var newTags = allNewTags.ToList();
                        return (enumerable.All(newTags.Contains) && f.Id != v.Id && enumerable.Count() == newTags.Count());
                    });
                })
                .WithMessage("Should be unique.");

            GetResourceAll = GetAll;

            GetResourceById = GetById;

            UpdateResource = Update;

            CreateResource = Create;

            DeleteResource = Delete;

            Get["/test"] = x => Test();

            Post["/test"] = x => TestWithNewModel();

            Get["schema"] = x => GetTemplates();
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

        private void Delete(int id)
        {
            _formatService.Delete(id);
        }

        private Response GetTemplates()
        {
            return CustomFormatService.Templates.SelectMany(t =>
            {
                return t.Value.Select(m =>
                {
                    var r = m.ToResource();
                    r.Simplicity = t.Key;
                    return r;
                });
            }).AsResponse();
        }

        private CustomFormatTestResource Test()
        {
            var parsed = _parsingService.ParseMovieInfo((string) Request.Query.title, new List<object>());
            if (parsed == null)
            {
                return null;
            }
            return new CustomFormatTestResource
            {
                Matches = _parsingService.MatchFormatTags(parsed).ToResource(),
                MatchedFormats = parsed.Quality.CustomFormats.ToResource()
            };
        }

        private CustomFormatTestResource TestWithNewModel()
        {
            var queryTitle = (string) Request.Query.title;

            var resource = ReadResourceFromRequest();

            var model = resource.ToModel();
            model.Name = model.Name += " (New)";

            var parsed = _parsingService.ParseMovieInfo(queryTitle, new List<object>{model});
            if (parsed == null)
            {
                return null;
            }
            return new CustomFormatTestResource
            {
                Matches = _parsingService.MatchFormatTags(parsed).ToResource(),
                MatchedFormats = parsed.Quality.CustomFormats.ToResource()
            };
        }
    }
}
