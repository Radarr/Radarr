using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.HealthCheck
{
    public interface IHealthCheckService
    {
        List<HealthCheck> Results();
    }

    public class HealthCheckService : IHealthCheckService,
                                      IExecute<CheckHealthCommand>,
                                      IHandleAsync<ApplicationStartedEvent>,
                                      IHandleAsync<IEvent>
    {
        private readonly IProvideHealthCheck[] _healthChecks;
        private readonly IProvideHealthCheck[] _startupHealthChecks;
        private readonly IProvideHealthCheck[] _scheduledHealthChecks;
        private readonly Dictionary<Type, EventDrivenHealthCheck[]> _eventDrivenHealthChecks;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICacheManager _cacheManager;
        private readonly Logger _logger;

        private readonly ICached<HealthCheck> _healthCheckResults;

        public HealthCheckService(IEnumerable<IProvideHealthCheck> healthChecks,
                                  IEventAggregator eventAggregator,
                                  ICacheManager cacheManager,
                                  Logger logger)
        {
            _healthChecks = healthChecks.ToArray();
            _eventAggregator = eventAggregator;
            _cacheManager = cacheManager;
            _logger = logger;

            _healthCheckResults = _cacheManager.GetCache<HealthCheck>(GetType());

            _startupHealthChecks = _healthChecks.Where(v => v.CheckOnStartup).ToArray();
            _scheduledHealthChecks = _healthChecks.Where(v => v.CheckOnSchedule).ToArray();
            _eventDrivenHealthChecks = GetEventDrivenHealthChecks();
        }

        public List<HealthCheck> Results()
        {
            return _healthCheckResults.Values.ToList();
        }

        private Dictionary<Type, EventDrivenHealthCheck[]> GetEventDrivenHealthChecks()
        {
            return _healthChecks
                .SelectMany(h => h.GetType().GetAttributes<CheckOnAttribute>().Select(a => Tuple.Create(a.EventType, new EventDrivenHealthCheck(h, a.Condition))))
                .GroupBy(t => t.Item1, t => t.Item2)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }

        private void PerformHealthCheck(IProvideHealthCheck[] healthChecks)
        {
            var results = healthChecks.Select(c => c.Check())
                                       .ToList();

            foreach (var result in results)
            {
                if (result.Type == HealthCheckResult.Ok)
                {
                    _healthCheckResults.Remove(result.Source.Name);
                }
                else
                {
                    if (_healthCheckResults.Find(result.Source.Name) == null)
                    {
                        _eventAggregator.PublishEvent(new HealthCheckFailedEvent(result));
                    }

                    _healthCheckResults.Set(result.Source.Name, result);
                }
            }

            _eventAggregator.PublishEvent(new HealthCheckCompleteEvent());
        }

        public void Execute(CheckHealthCommand message)
        {
            if (message.Trigger == CommandTrigger.Manual)
            {
                PerformHealthCheck(_healthChecks);
            }
            else
            {
                PerformHealthCheck(_scheduledHealthChecks);
            }
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            PerformHealthCheck(_startupHealthChecks);
        }

        public void HandleAsync(IEvent message)
        {
            if (message is HealthCheckCompleteEvent)
            {
                return;
            }

            EventDrivenHealthCheck[] checks;
            if (!_eventDrivenHealthChecks.TryGetValue(message.GetType(), out checks))
            {
                return;
            }

            var filteredChecks = new List<IProvideHealthCheck>();
            var healthCheckResults = _healthCheckResults.Values.ToList();

            foreach (var eventDrivenHealthCheck in checks)
            {
                if (eventDrivenHealthCheck.Condition == CheckOnCondition.Always)
                {
                    filteredChecks.Add(eventDrivenHealthCheck.HealthCheck);
                    continue;
                }

                var healthCheckType = eventDrivenHealthCheck.HealthCheck.GetType();

                if (eventDrivenHealthCheck.Condition == CheckOnCondition.FailedOnly &&
                    healthCheckResults.Any(r => r.Source == healthCheckType))
                {
                    filteredChecks.Add(eventDrivenHealthCheck.HealthCheck);
                    continue;
                }

                if (eventDrivenHealthCheck.Condition == CheckOnCondition.SuccessfulOnly &&
                         healthCheckResults.None(r => r.Source == healthCheckType))
                {
                    filteredChecks.Add(eventDrivenHealthCheck.HealthCheck);
                }
            }

            // TODO: Add debounce
            PerformHealthCheck(filteredChecks.ToArray());
        }
    }
}
