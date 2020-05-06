namespace VersOne.Epub.Schema
{
    public enum PageProgressionDirection
    {
        DEFAULT = 1,
        LEFT_TO_RIGHT,
        RIGHT_TO_LEFT,
        UNKNOWN
    }

    internal static class PageProgressionDirectionParser
    {
        public static PageProgressionDirection Parse(string stringValue)
        {
            switch (stringValue.ToLowerInvariant())
            {
                case "default":
                    return PageProgressionDirection.DEFAULT;
                case "ltr":
                    return PageProgressionDirection.LEFT_TO_RIGHT;
                case "rtl":
                    return PageProgressionDirection.RIGHT_TO_LEFT;
                default:
                    return PageProgressionDirection.UNKNOWN;
            }
        }
    }
}
