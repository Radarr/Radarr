using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class LanguageSpecification : CustomFormatSpecificationBase
    {
        public override int Order => 3;
        public override string ImplementationName => "Language";

        [FieldDefinition(1, Label = "Language", Type = FieldType.Select, SelectOptions = typeof(LanguageFieldConverter))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(ParsedMovieInfo movieInfo)
        {
            var comparedLanguage = movieInfo != null && Name == "Original" && movieInfo.ExtraInfo.ContainsKey("OriginalLanguage")
                ? (Language)movieInfo.ExtraInfo["OriginalLanguage"]
                : (Language)Value;
            return movieInfo?.Languages?.Contains(comparedLanguage) ?? false;
        }
    }
}
