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
            IgnoreOnMonoVersions("5.12", "5.14");

            var config = HostConfig.Get(1);
            config.LogLevel = "Trace";
            HostConfig.Put(config);

            var resultGet = Movies.All();

            var logFile = "radarr.trace.txt";
            var logLines = Logs.GetLogFileLines(logFile);

            var resultPost = Movies.InvalidPost(new Radarr.Api.V2.Movies.MovieResource());

            // Skip 2 and 1 to ignore the logs endpoint
            logLines = Logs.GetLogFileLines(logFile).Skip(logLines.Length + 2).ToArray();
            Array.Resize(ref logLines, logLines.Length - 1);

            logLines.Should().Contain(v => v.Contains("|Trace|Http|Req") && v.Contains("/api/v2/movie/"));
            logLines.Should().Contain(v => v.Contains("|Trace|Http|Res") && v.Contains("/api/v2/movie/: 400.BadRequest"));
            logLines.Should().Contain(v => v.Contains("|Debug|Api|") && v.Contains("/api/v2/movie/: 400.BadRequest"));
        }
    }
}