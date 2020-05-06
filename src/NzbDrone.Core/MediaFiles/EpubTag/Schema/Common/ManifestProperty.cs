namespace VersOne.Epub.Schema
{
    public enum ManifestProperty
    {
        COVER_IMAGE = 1,
        MATHML,
        NAV,
        REMOTE_RESOURCES,
        SCRIPTED,
        SVG,
        UNKNOWN
    }

    public static class ManifestPropertyParser
    {
        public static ManifestProperty Parse(string stringValue)
        {
            switch (stringValue.ToLowerInvariant())
            {
                case "cover-image":
                    return ManifestProperty.COVER_IMAGE;
                case "mathml":
                    return ManifestProperty.MATHML;
                case "nav":
                    return ManifestProperty.NAV;
                case "remote-resources":
                    return ManifestProperty.REMOTE_RESOURCES;
                case "scripted":
                    return ManifestProperty.SCRIPTED;
                case "svg":
                    return ManifestProperty.SVG;
                default:
                    return ManifestProperty.UNKNOWN;
            }
        }
    }
}
