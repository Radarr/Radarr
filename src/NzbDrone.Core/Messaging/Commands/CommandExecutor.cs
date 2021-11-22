using System;
using System.Threading;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ProgressMessaging;

namespace NzbDrone.Core.Messaging.Commands
{
    public class CommandExecutor : IHandle<ApplicationStartedEvent>,
                                   IHandle<ApplicationShutdownRequested>
    {
        private const int THREAD_UPPER_BOUND = 10;
        private const int THREAD_LOWER_BOUND = 2;
        private const int THREAD_LIMIT = 2;

        private readonly Logger _logger;
        private readonly IServiceFactory _serviceFactory;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IEventAggregator _eventAggregator;

        private static CancellationTokenSource _cancellationTokenSource;

        public CommandExecutor(IServiceFactory serviceFactory,
                               IManageCommandQueue commandQueueManager,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _logger = logger;
            _serviceFactory = serviceFactory;
            _commandQueueManager = commandQueueManager;
            _eventAggregator = eventAggregator;
        }

        private void ExecuteCommands()
        {
            try
            {
                foreach (var command in _commandQueueManager.Queue(_cancellationTokenSource.Token))
                {
                    try
                    {
                        ExecuteCommand((dynamic)command.Body, command);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error occurred while executing task {0}", command.Name);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Trace("Stopped one command execution pipeline");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unknown error in thread");
            }
        }

        private void ExecuteCommand<TCommand>(TCommand command, CommandModel commandModel)
            where TCommand : Command
        {
            IExecute<TCommand> handler = null;

            try
            {
                handler = (IExecute<TCommand>)_serviceFactory.Build(typeof(IExecute<TCommand>));

                _logger.Trace("{0} -> {1}", command.GetType().Name, handler.GetType().Name);

                _commandQueueManager.Start(commandModel);
                BroadcastCommandUpdate(commandModel);

                if (ProgressMessageContext.CommandModel == null)
                {
                    ProgressMessageContext.CommandModel = commandModel;
                }

                handler.Execute(command);

                _commandQueueManager.Complete(commandModel, command.CompletionMessage ?? commandModel.Message);
            }
            catch (CommandFailedException ex)
            {
                _commandQueueManager.SetMessage(commandModel, "Failed");
                _commandQueueManager.Fail(commandModel, ex.Message, ex);
                throw;
            }
            catch (Exception ex)
            {
                _commandQueueManager.SetMessage(commandModel, "Failed");
                _commandQueueManager.Fail(commandModel, "Failed", ex);
                throw;
            }
            finally
            {
                BroadcastCommandUpdate(commandModel);

                _eventAggregator.PublishEvent(new CommandExecutedEvent(commandModel));

                if (ProgressMessageContext.CommandModel == commandModel)
                {
                    ProgressMessageContext.CommandModel = null;
                }

                if (handler != null)
                {
                    _logger.Trace("{0} <- {1} [{2}]", command.GetType().Name, handler.GetType().Name, commandModel.Duration.ToString());
                }
            }
        }

        private void BroadcastCommandUpdate(CommandModel command)
        {
            if (command.Body.SendUpdatesToClient)
            {
                _eventAggregator.PublishEvent(new CommandUpdatedEvent(command));
            }
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var envLimit = Environment.GetEnvironmentVariable("THREAD_LIMIT") ?? $"{THREAD_LIMIT}";
            int threadLimit = THREAD_LIMIT;
            if (int.TryParse(envLimit, out int parsedLimit))
            {
                threadLimit = parsedLimit;
            }

            threadLimit = Math.Max(THREAD_LOWER_BOUND, threadLimit);
            threadLimit = Math.Min(THREAD_UPPER_BOUND, threadLimit);

            _logger.Info("Starting {} threads for tasks.", threadLimit);

            for (int i = 0; i < threadLimit + 1; i++)
            {
                var thread = new Thread(ExecuteCommands);
                thread.Start();
            }
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            _logger.Info("Shutting down task execution");
            _cancellationTokenSource.Cancel(true);
        }
    }
}
