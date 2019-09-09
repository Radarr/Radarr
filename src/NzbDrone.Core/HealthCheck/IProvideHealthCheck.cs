using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.HealthCheck
{
    public interface IProvideHealthCheck
    {
        HealthCheck Check();
        bool CheckOnStartup { get; }
        bool CheckOnSchedule { get; }
    }

    public interface IProvideHealthCheckWithMessage : IProvideHealthCheck
    {
        HealthCheck Check(IEvent message);
    }
}
