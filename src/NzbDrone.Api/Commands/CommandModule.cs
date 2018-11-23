using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Radarr.Http.Extensions;
using Radarr.Http.Validation;
using NzbDrone.Common;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ProgressMessaging;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.Mapping;


namespace NzbDrone.Api.Commands
{
    public class CommandModule : RadarrRestModuleWithSignalR<CommandResource, CommandModel>, IHandle<CommandUpdatedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IServiceFactory _serviceFactory;
        private readonly Logger _logger;

        public CommandModule(IManageCommandQueue commandQueueManager,
                             IBroadcastSignalRMessage signalRBroadcaster,
                             IServiceFactory serviceFactory,
                             Logger logger)
            : base(signalRBroadcaster)
        {
            _commandQueueManager = commandQueueManager;
            _serviceFactory = serviceFactory;
            _logger = logger;

            GetResourceById = GetCommand;
            CreateResource = StartCommand;
            GetResourceAll = GetStartedCommands;

            PostValidator.RuleFor(c => c.Name).NotBlank();
        }

        private CommandResource GetCommand(int id)
        {
            return _commandQueueManager.Get(id).ToResource();
        }

        private int StartCommand(CommandResource commandResource)
        {
            var commandType = _serviceFactory.GetImplementations(typeof(Command))
                                             .SingleOrDefault(c => c.Name.Replace("Command", "").Equals(commandResource.Name, StringComparison.InvariantCultureIgnoreCase));

            if (commandType == null)
            {
                _logger.Error("Found no matching command for {0}", commandResource.Name);
                return 0;
            }

            dynamic command = Request.Body.FromJson(commandType);
            command.Trigger = CommandTrigger.Manual;

            var trackedCommand = _commandQueueManager.Push(command, CommandPriority.Normal, CommandTrigger.Manual);
            return trackedCommand.Id;
        }

        private List<CommandResource> GetStartedCommands()
        {
            return _commandQueueManager.GetStarted().ToResource();
        }

        public void Handle(CommandUpdatedEvent message)
        {
            if (message.Command.Body.SendUpdatesToClient)
            {
                BroadcastResourceChange(ModelAction.Updated, message.Command.ToResource());
            }
        }
    }
}
