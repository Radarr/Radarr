namespace NzbDrone.Core.CustomFormats
{
    public class FullReleaseTitleSpecification : RegexSpecificationBase
    {
        public override int Order => 1;
        public override string ImplementationName => "Full Release Title";
        public override string InfoLink => "https://wiki.servarr.com/radarr/settings#custom-formats-2";

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return MatchString(input.MovieInfo?.ReleaseTitle) || MatchString(input.Filename);
        }
    }
}
