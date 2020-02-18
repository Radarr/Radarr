using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.CustomFormats
{
    public class QualityModifierSpecification : CustomFormatSpecificationBase
    {
        public override int Order => 7;
        public override string ImplementationName => "Quality Modifier";

        [FieldDefinition(1, Label = "Quality Modifier", Type = FieldType.Select, SelectOptions = typeof(Modifier))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(ParsedMovieInfo movieInfo)
        {
            return (movieInfo?.Quality?.Quality?.Modifier ?? (int)Modifier.NONE) == (Modifier)Value;
        }
    }
}
