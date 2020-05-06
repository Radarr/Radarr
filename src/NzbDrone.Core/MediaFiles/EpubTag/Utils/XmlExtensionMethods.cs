using System.Xml.Linq;

namespace VersOne.Epub.Utils
{
    public static class XmlExtensionMethods
    {
        public static string GetLowerCaseLocalName(this XAttribute xAttribute)
        {
            return xAttribute.Name.LocalName.ToLowerInvariant();
        }

        public static string GetLowerCaseLocalName(this XElement xElement)
        {
            return xElement.Name.LocalName.ToLowerInvariant();
        }

        public static bool CompareNameTo(this XElement xElement, string value)
        {
            return xElement.Name.LocalName.CompareOrdinalIgnoreCase(value);
        }

        public static bool CompareValueTo(this XAttribute xAttribute, string value)
        {
            return xAttribute.Value.CompareOrdinalIgnoreCase(value);
        }
    }
}
