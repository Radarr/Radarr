using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.Http
{
    public class HttpUriFixture : TestBase
    {
        [TestCase("abc://my_host.com:8080/root/api/")]
        [TestCase("abc://my_host.com:8080//root/api/")]
        [TestCase("abc://my_host.com:8080/root//api/")]
        [TestCase("abc://[::1]:8080/root//api/")]
        public void should_parse(string uri)
        {
            var newUri = new HttpUri(uri);
            newUri.FullUri.Should().Be(uri);
        }

        [Test]
        public void should_parse_uri_template_regardless_of_culture()
        {
            var currentCulture = CultureInfo.CurrentCulture;
            var currentUiCulture = CultureInfo.CurrentUICulture;

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");

            try
            {
                var uri = "https://api.themoviedb.org/{api}/{route}/{id}{secondaryRoute}";
                var newUri = new HttpUri(uri);

                newUri.FullUri.Should().Be(uri);
                newUri.Scheme.Should().Be("https");
                newUri.Host.Should().Be("api.themoviedb.org");
                newUri.Path.Should().Be("/{api}/{route}/{id}{secondaryRoute}");
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
                CultureInfo.CurrentUICulture = currentUiCulture;
            }
        }

        [TestCase("", "", "")]
        [TestCase("/", "", "/")]
        [TestCase("base", "", "base")]
        [TestCase("/base", "", "/base")]
        [TestCase("/base/", "", "/base/")]
        [TestCase("", "relative", "relative")]
        [TestCase("", "/relative", "/relative")]
        [TestCase("/", "relative", "/relative")]
        [TestCase("/", "/relative", "/relative")]
        [TestCase("base", "relative", "relative")]
        [TestCase("base", "/relative", "/relative")]
        [TestCase("/base", "relative", "/relative")]
        [TestCase("/base", "/relative", "/relative")]
        [TestCase("/base/", "relative", "/base/relative")]
        [TestCase("/base/", "/relative", "/relative")]
        [TestCase("base/sub", "relative", "base/relative")]
        [TestCase("base/sub", "/relative", "/relative")]
        [TestCase("/base/sub", "relative", "/base/relative")]
        [TestCase("/base/sub", "/relative", "/relative")]
        [TestCase("/base/sub/", "relative", "/base/sub/relative")]
        [TestCase("/base/sub/", "/relative", "/relative")]
        [TestCase("abc://host.com:8080/root/file.xml", "relative/path", "abc://host.com:8080/root/relative/path")]
        [TestCase("abc://host.com:8080/root/file.xml", "/relative/path", "abc://host.com:8080/relative/path")]
        [TestCase("abc://host.com:8080/root/file.xml?query=1#fragment", "relative/path", "abc://host.com:8080/root/relative/path")]
        [TestCase("abc://host.com:8080/root/file.xml?query=1#fragment", "/relative/path", "abc://host.com:8080/relative/path")]
        [TestCase("abc://host.com:8080/root/api", "relative/path", "abc://host.com:8080/root/relative/path")]
        [TestCase("abc://host.com:8080/root/api", "/relative/path", "abc://host.com:8080/relative/path")]
        [TestCase("abc://host.com:8080/root/api/", "relative/path", "abc://host.com:8080/root/api/relative/path")]
        [TestCase("abc://host.com:8080/root/api/", "/relative/path", "abc://host.com:8080/relative/path")]
        [TestCase("abc://host.com:8080/root/api/", "//otherhost.com/path", "abc://otherhost.com/path")]
        public void should_combine_uri(string basePath, string relativePath, string expected)
        {
            var newUri = new HttpUri(basePath) + new HttpUri(relativePath);
            newUri.FullUri.Should().Be(expected);
        }

        [TestCase("", "", "")]
        [TestCase("/", "", "/")]
        [TestCase("base", "", "base")]
        [TestCase("/base", "", "/base")]
        [TestCase("/base/", "", "/base/")]
        [TestCase("", "relative", "relative")]
        [TestCase("", "/relative", "/relative")]
        [TestCase("/", "relative", "/relative")]
        [TestCase("/", "/relative", "/relative")]
        [TestCase("base", "relative", "base/relative")]
        [TestCase("base", "/relative", "base/relative")]
        [TestCase("/base", "relative", "/base/relative")]
        [TestCase("/base", "/relative", "/base/relative")]
        [TestCase("/base/", "relative", "/base/relative")]
        [TestCase("/base/", "/relative", "/base/relative")]
        [TestCase("base/sub", "relative", "base/sub/relative")]
        [TestCase("base/sub", "/relative", "base/sub/relative")]
        [TestCase("/base/sub", "relative", "/base/sub/relative")]
        [TestCase("/base/sub", "/relative", "/base/sub/relative")]
        [TestCase("/base/sub/", "relative", "/base/sub/relative")]
        [TestCase("/base/sub/", "/relative", "/base/sub/relative")]
        [TestCase("/base/sub/", "relative/", "/base/sub/relative/")]
        [TestCase("/base/sub/", "/relative/", "/base/sub/relative/")]
        [TestCase("abc://host.com:8080/root/file.xml", "relative/path", "abc://host.com:8080/root/file.xml/relative/path")]
        [TestCase("abc://host.com:8080/root/file.xml", "/relative/path", "abc://host.com:8080/root/file.xml/relative/path")]
        [TestCase("abc://host.com:8080/root/file.xml?query=1#fragment", "relative/path", "abc://host.com:8080/root/file.xml/relative/path?query=1#fragment")]
        [TestCase("abc://host.com:8080/root/file.xml?query=1#fragment", "/relative/path", "abc://host.com:8080/root/file.xml/relative/path?query=1#fragment")]
        [TestCase("abc://host.com:8080/root/api", "relative/path", "abc://host.com:8080/root/api/relative/path")]
        [TestCase("abc://host.com:8080/root/api", "/relative/path", "abc://host.com:8080/root/api/relative/path")]
        [TestCase("abc://host.com:8080/root/api/", "relative/path", "abc://host.com:8080/root/api/relative/path")]
        [TestCase("abc://host.com:8080/root/api/", "/relative/path", "abc://host.com:8080/root/api/relative/path")]
        public void should_combine_relative_path(string basePath, string relativePath, string expected)
        {
            var newUri = new HttpUri(basePath).CombinePath(relativePath);

            newUri.FullUri.Should().Be(expected);
        }
    }
}
