using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using NUnit.Framework;
using NzbDrone.Host;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class StringResultOutputFormatterFixture
    {
        private sealed class SampleResource
        {
        }

        private static IReadOnlyList<string> GetSupportedContentTypes<T>(IOutputFormatter formatter)
        {
            return ((IApiResponseTypeMetadataProvider)formatter).GetSupportedContentTypes(null, typeof(T));
        }

        [Test]
        public void should_offer_text_plain_for_string_results()
        {
            GetSupportedContentTypes<string>(new StringResultOutputFormatter())
                .Should()
                .BeEquivalentTo("text/plain");
        }

        [Test]
        public void should_offer_text_plain_for_actions_declared_as_object()
        {
            GetSupportedContentTypes<object>(new StringResultOutputFormatter())
                .Should()
                .BeEquivalentTo("text/plain");
        }

        [Test]
        public void should_not_offer_text_plain_for_other_types()
        {
            GetSupportedContentTypes<SampleResource>(new StringResultOutputFormatter())
                .Should()
                .BeNullOrEmpty();
        }

        [Test]
        public void should_differ_from_the_framework_formatter_it_replaces()
        {
            GetSupportedContentTypes<SampleResource>(new StringOutputFormatter())
                .Should()
                .BeEquivalentTo("text/plain");
        }
    }
}
