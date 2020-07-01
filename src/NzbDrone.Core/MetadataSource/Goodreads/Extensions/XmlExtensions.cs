using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    internal static class XmlExtensions
    {
        public static string ElementAsString(this XElement element, XName name, bool trim = false)
        {
            var el = element.Element(name);

            return string.IsNullOrWhiteSpace(el?.Value)
                ? null
                : (trim ? el.Value.Trim() : el.Value);
        }

        public static long ElementAsLong(this XElement element, XName name)
        {
            var el = element.Element(name);
            return long.TryParse(el?.Value, out long value) ? value : default(long);
        }

        public static long? ElementAsNullableLong(this XElement element, XName name)
        {
            var el = element.Element(name);
            return long.TryParse(el?.Value, out long value) ? new long?(value) : null;
        }

        public static int ElementAsInt(this XElement element, XName name)
        {
            var el = element.Element(name);
            return int.TryParse(el?.Value, out int value) ? value : default(int);
        }

        public static int? ElementAsNullableInt(this XElement element, XName name)
        {
            var el = element.Element(name);
            return int.TryParse(el?.Value, out int value) ? new int?(value) : null;
        }

        public static decimal ElementAsDecimal(this XElement element, XName name)
        {
            var el = element.Element(name);
            return decimal.TryParse(el?.Value, out decimal value) ? value : default(decimal);
        }

        public static decimal? ElementAsNullableDecimal(this XElement element, XName name)
        {
            var el = element.Element(name);
            return decimal.TryParse(el?.Value, out decimal value) ? new decimal?(value) : null;
        }

        public static DateTime? ElementAsDate(this XElement element, XName name)
        {
            var el = element.Element(name);
            return DateTime.TryParseExact(el?.Value, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)
                ? new DateTime?(date)
                : null;
        }

        public static DateTime? ElementAsDateTime(this XElement element, XName name)
        {
            var dateElement = element.Element(name);
            if (dateElement != null)
            {
                var value = dateElement.Value;

                // The Goodreads date includes the timezone as -hhmm whereas C# wants it to be -hh:mm
                // This regex corrects the format and hopefully doesn't mess anything else up...
                var validDateFormat = Regex.Replace(value, @"(.*) ([+-]\d{2})(\d{2}) (.*)", "$1 $2:$3 $4");

                DateTime localDate;
                if (DateTime.TryParseExact(
                    validDateFormat,
                    "ddd MMM dd HH:mm:ss zzz yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out localDate))
                {
                    return localDate.ToUniversalTime();
                }
                else if (DateTime.TryParseExact(
                    validDateFormat,
                    "yyyy-MM-ddTHH:mm:sszzz",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out localDate))
                {
                    return localDate.ToUniversalTime();
                }
            }

            return null;
        }

        public static DateTime? ElementAsMonthYear(this XElement element, XName name)
        {
            var el = element.Element(name);
            return DateTime.TryParseExact(el?.Value, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)
                ? new DateTime?(date)
                : null;
        }

        /// <summary>
        /// Goodreads sometimes returns dates as three separate fields.
        /// This method parses out each one and returns a date object.
        /// </summary>
        /// <param name="element">The parent element of the date elements.</param>
        /// <param name="prefix">The common prefix for the three Goodreads date elements.</param>
        /// <returns>A date object after parsing the three Goodreads date fields.</returns>
        public static DateTime? ElementAsMultiDateField(this XElement element, string prefix)
        {
            var publicationYear = element.ElementAsNullableInt(prefix + "_year");
            var publicationMonth = element.ElementAsNullableInt(prefix + "_month");
            var publicationDay = element.ElementAsNullableInt(prefix + "_day");

            if (!publicationYear.HasValue &&
                !publicationMonth.HasValue &&
                !publicationDay.HasValue)
            {
                return null;
            }

            if (!publicationYear.HasValue || publicationYear <= 0)
            {
                return null;
            }

            if (!publicationDay.HasValue)
            {
                publicationDay = 1;
            }

            if (!publicationMonth.HasValue)
            {
                publicationMonth = 1;
            }

            try
            {
                return new DateTime(publicationYear.Value, publicationMonth.Value, publicationDay.Value, 0, 0, 0, DateTimeKind.Utc);
            }
            catch
            {
                return null;
            }
        }

        public static bool ElementAsBool(this XElement element, XName name)
        {
            var el = element.Element(name);
            return bool.TryParse(el?.Value, out bool value) ? value : false;
        }

        public static List<T> ParseChildren<T>(this XElement element, XName parentName, XName childName)
            where T : GoodreadsResource, new()
        {
            return ParseChildren(
                element,
                parentName,
                childName,
                (childElement) =>
            {
                var child = new T();
                child.Parse(childElement);
                return child;
            });
        }

        public static List<T> ParseChildren<T>(this XElement element, XName parentName, XName childName, Func<XElement, T> parseChild)
        {
            var parentElement = element.Element(parentName);
            if (parentElement != null)
            {
                var childElements = parentElement.Descendants(childName);
                if (childElements.Any())
                {
                    var children = new List<T>();

                    foreach (var childElement in childElements)
                    {
                        children.Add(parseChild(childElement));
                    }

                    return children;
                }
            }

            return null;
        }

        public static List<T> ParseChildren<T>(this XElement element)
            where T : GoodreadsResource, new()
        {
            var childElements = element.Elements();
            if (childElements.Any())
            {
                var children = new List<T>();

                foreach (var childElement in childElements)
                {
                    var child = new T();
                    child.Parse(childElement);
                    children.Add(child);
                }

                return children;
            }

            return null;
        }

        public static string AttributeAsString(this XElement element, XName attributeName)
        {
            var attr = element.Attribute(attributeName);
            return string.IsNullOrWhiteSpace(attr?.Value) ? null : attr.Value;
        }

        public static int AttributeAsInt(this XElement element, XName attributeName)
        {
            var attr = element.Attribute(attributeName);
            return int.TryParse(attr?.Value, out int value) ? value : default(int);
        }

        public static long? AttributeAsNullableLong(this XElement element, XName attributeName)
        {
            var attr = element.Attribute(attributeName);
            return long.TryParse(attr?.Value, out long value) ? new long?(value) : null;
        }

        public static bool AttributeAsBool(this XElement element, XName attributeName)
        {
            var attr = element.Attribute(attributeName);
            return bool.TryParse(attr?.Value, out bool value) ? value : false;
        }
    }
}
