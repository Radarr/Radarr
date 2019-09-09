using FluentAssertions;
using NUnit.Framework;
using Lidarr.Api.V1.Indexers;
using System.Net;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class ReleasePushFixture : IntegrationTest
    {
        [Test]
        public void should_have_utc_date()
        {
            var body = new Dictionary<string, object>();
            body.Add("title", "The Artist - The Album (2008) [FLAC]");
            body.Add("protocol", "Torrent");
            body.Add("downloadUrl", "https://lidarr.audio/test.torrent");
            body.Add("publishDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ssZ", CultureInfo.InvariantCulture));

            var request = ReleasePush.BuildRequest();
            request.AddBody(body);
            var result = ReleasePush.Post<ReleaseResource>(request, HttpStatusCode.OK);

            result.Should().NotBeNull();
            result.AgeHours.Should().BeApproximately(0, 0.1);
        }
    }
}
