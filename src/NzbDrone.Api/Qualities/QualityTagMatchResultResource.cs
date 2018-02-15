using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Qualities
{
    public class QualityTagMatchResultResource : RestResource
    {
        public QualityDefinitionResource QualityDefinition { get; set; }
        public Dictionary<string, bool> Matches { get; set; }
    }

    public class QualityDefinitionTestResource : RestResource
    {
        public List<QualityTagMatchResultResource> Matches { get; set; }
        public QualityDefinitionResource BestMatch { get; set; }
    }
    
    public static class QualityTagMatchResultResourceMapper
    {
        public static QualityTagMatchResultResource ToResource(this QualityTagMatchResult model)
        {
            if (model == null) return null;

            return new QualityTagMatchResultResource
            {
                QualityDefinition = model.QualityDefinition.ToResource(),
                Matches = model.Matches.Select(m => new {m.Key.Raw, m.Value}).ToDictionary(m => m.Raw, m => m.Value)
            };
        }

        

        public static List<QualityTagMatchResultResource> ToResource(this IList<QualityTagMatchResult> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
