using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class HttpLogFixture : IntegrationTest
    {
        [Test]
        public void should_log_on_error()
        {
            var config = HostConfig.Get(1);
            config.LogLevel = "Trace";
            HostConfig.Put(config);

            var resultGet = Artist.All();

            var logFile = "Lidarr.trace.txt";
            var logLines = Logs.GetLogFileLines(logFile);

            var result = Artist.InvalidPost(new Lidarr.Api.V1.Artist.ArtistResource());

            // Skip 2 and 1 to ignore the logs endpoint
            logLines = Logs.GetLogFileLines(logFile).Skip(logLines.Length + 2).ToArray();
            Array.Resize(ref logLines, logLines.Length - 1);

            logLines.Should().Contain(v => v.Contains("|Trace|Http|Req") && v.Contains("/api/v1/artist/"));
            logLines.Should().Contain(v => v.Contains("|Trace|Http|Res") && v.Contains("/api/v1/artist/: 400.BadRequest"));
            logLines.Should().Contain(v => v.Contains("|Debug|Api|") && v.Contains("/api/v1/artist/: 400.BadRequest"));
        }
    }
}
