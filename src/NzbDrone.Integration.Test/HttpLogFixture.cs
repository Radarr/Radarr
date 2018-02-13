using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Movie;

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

            var logFile = Path.Combine(_runner.AppData, "logs", "radarr.trace.txt");
            var logLines = File.ReadAllLines(logFile);

            var result = Movies.InvalidPost(new MovieResource());

            logLines = File.ReadAllLines(logFile).Skip(logLines.Length).ToArray();

            logLines.Should().Contain(v => v.Contains("|Trace|Http|Req"));
            logLines.Should().Contain(v => v.Contains("|Trace|Http|Res"));
            logLines.Should().Contain(v => v.Contains("|Debug|Api|"));
        }
    }
}
