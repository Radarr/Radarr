using NzbDrone.Common.Messaging;
namespace NzbDrone.Core.HealthCheck
{
    public interface ICheckOnCondition<TEvent>
    {
        bool ShouldCheckOnEvent(TEvent message);
    }

    public interface IProvideHealthCheckWithMessage : IProvideHealthCheck
    {
        HealthCheck Check(IEvent message);
    }
}
