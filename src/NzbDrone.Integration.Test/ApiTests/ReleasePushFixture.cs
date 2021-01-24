using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Readarr.Api.V1.Indexers;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class ReleasePushFixture : IntegrationTest
    {
        [Test]
        public void should_have_utc_date()
        {
            var body = new Dictionary<string, object>();
            body.Add("title", "The Author - The Book (2008) [FLAC]");
            body.Add("protocol", "Torrent");
            body.Add("downloadUrl", "https://readarr.com/test.torrent");
            body.Add("publishDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ssZ", CultureInfo.InvariantCulture));

            var request = ReleasePush.BuildRequest();
            request.AddJsonBody(body);
            var result = ReleasePush.Post<ReleaseResource>(request, HttpStatusCode.OK);

            result.Should().NotBeNull();
            result.AgeHours.Should().BeApproximately(0, 0.1);
        }
    }
}
