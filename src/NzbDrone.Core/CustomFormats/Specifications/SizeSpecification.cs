using System.Collections.Generic;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class SizeSpecification : CustomFormatSpecificationBase
    {
        public override int Order => 8;
        public override string ImplementationName => "Size";

        [FieldDefinition(1, Label = "Minimum Size", HelpText = "Release must be greater than this size", Unit = "GB", Type = FieldType.Number)]
        public double Min { get; set; }

        [FieldDefinition(1, Label = "Maximum Size", HelpText = "Release must be less than or equal to this size", Unit = "GB", Type = FieldType.Number)]
        public double Max { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(ParsedMovieInfo movieInfo)
        {
            var size = (movieInfo?.ExtraInfo?.GetValueOrDefault("Size", 0.0) as long?) ?? 0;

            return size > Min.Gigabytes() && size <= Max.Gigabytes();
        }
    }
}
