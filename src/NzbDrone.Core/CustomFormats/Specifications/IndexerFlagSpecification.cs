using System.Collections.Generic;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class IndexerFlagSpecification : CustomFormatSpecificationBase
    {
        public override int Order => 4;
        public override string ImplementationName => "Indexer Flag";

        [FieldDefinition(1, Label = "Flag", Type = FieldType.Select, SelectOptions = typeof(IndexerFlags))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(ParsedMovieInfo movieInfo)
        {
            var flags = movieInfo?.ExtraInfo?.GetValueOrDefault("IndexerFlags") as IndexerFlags?;
            return flags?.HasFlag((IndexerFlags)Value) == true;
        }
    }
}
