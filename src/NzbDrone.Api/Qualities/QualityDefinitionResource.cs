using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Qualities
{
    public class QualityDefinitionResource : RestResource
    {
        public Quality Quality { get; set; }
        public List<string> QualityTags { get; set; }

        public string Title { get; set; }

        public int Weight { get; set; }

        public double? MinSize { get; set; }
        public double? MaxSize { get; set; }

        public QualityDefinitionResource ParentQualityDefinition { get; set; }
    }

    public static class QualityDefinitionResourceMapper
    {
        public static QualityDefinitionResource ToResource(this QualityDefinition model)
        {
            if (model == null) return null;

            return new QualityDefinitionResource
            {
                Id = model.Id,

                Quality = model.Quality,
                QualityTags = model.QualityTags?.Select(t => t.Raw.ToUpper()).ToList(),

                Title = model.Title,

                Weight = model.Weight,

                MinSize = model.MinSize,
                MaxSize = model.MaxSize,

                ParentQualityDefinition = model.ParentQualityDefinition?.ToResource()
            };
        }

        public static QualityDefinition ToModel(this QualityDefinitionResource resource)
        {
            if (resource == null) return null;

            return new QualityDefinition
            {
                Id = resource.Id,

                Quality = resource.Quality,
                QualityTags = resource.QualityTags.Select(s => new QualityTag(s)).ToList(),

                Title = resource.Title,

                Weight = resource.Weight,

                MinSize = resource.MinSize,
                MaxSize = resource.MaxSize,

                ParentQualityDefinition = resource.ParentQualityDefinition?.ToModel()
            };
        }

        public static List<QualityDefinitionResource> ToResource(this IEnumerable<QualityDefinition> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
