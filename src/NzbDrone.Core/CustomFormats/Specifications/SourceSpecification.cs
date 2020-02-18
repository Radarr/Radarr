using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.CustomFormats
{
    public class SourceSpecification : CustomFormatSpecificationBase
    {
        public override int Order => 5;
        public override string ImplementationName => "Source";

        [FieldDefinition(1, Label = "Source", Type = FieldType.Select, SelectOptions = typeof(Source))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(ParsedMovieInfo movieInfo)
        {
            return (movieInfo?.Quality?.Quality?.Source ?? (int)Source.UNKNOWN) == (Source)Value;
        }
    }
}
