namespace NzbDrone.Core.CustomFormats
{
    public class ReleaseGroupSpecification : RegexSpecificationBase
    {
        public override int Order => 9;
        public override string ImplementationName => "Release Group";
        public override string InfoLink => "https://wiki.servarr.com/radarr/settings#custom-formats-2";

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return MatchString(input.MovieInfo?.ReleaseGroup);
        }
    }
}
