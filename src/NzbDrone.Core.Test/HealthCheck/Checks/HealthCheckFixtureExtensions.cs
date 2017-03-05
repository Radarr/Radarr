﻿using FluentAssertions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.HealthCheck;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    public static class HealthCheckFixtureExtensions
    {
        public static void ShouldBeOk(this Core.HealthCheck.HealthCheck result)
        {
            result.Type.Should().Be(HealthCheckResult.Ok);
        }

        public static void ShouldBeWarning(this Core.HealthCheck.HealthCheck result, string message = null)
        {
            result.Type.Should().Be(HealthCheckResult.Warning);

            if (message.IsNotNullOrWhiteSpace())
            {
                result.Message.Should().Contain(message);
            }
        }

        public static void ShouldBeError(this Core.HealthCheck.HealthCheck result, string message = null)
        {
            result.Type.Should().Be(HealthCheckResult.Error);

            if (message.IsNotNullOrWhiteSpace())
            {
                result.Message.Should().Contain(message);
            }
        }
    }
}
