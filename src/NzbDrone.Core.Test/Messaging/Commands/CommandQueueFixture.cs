using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.Download;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update.Commands;

namespace NzbDrone.Core.Test.Messaging.Commands
{
    [TestFixture]
    public class CommandQueueFixture : CoreTest<CommandQueue>
    {
        private void GivenStartedDiskCommand()
        {
            var commandModel = Builder<CommandModel>
                .CreateNew()
                .With(c => c.Name = "ProcessMonitoredDownloads")
                .With(c => c.Body = new ProcessMonitoredDownloadsCommand())
                .With(c => c.Status = CommandStatus.Started)
                .Build();

            Subject.Add(commandModel);
        }

        private void GivenStartedTypeExclusiveCommand()
        {
            var commandModel = Builder<CommandModel>
                .CreateNew()
                .With(c => c.Name = "ImportListSync")
                .With(c => c.Body = new ImportListSyncCommand())
                .With(c => c.Status = CommandStatus.Started)
                .Build();

            Subject.Add(commandModel);
        }

        private void GivenStartedExclusiveCommand()
        {
            var commandModel = Builder<CommandModel>
                .CreateNew()
                .With(c => c.Name = "ApplicationUpdate")
                .With(c => c.Body = new ApplicationUpdateCommand())
                .With(c => c.Status = CommandStatus.Started)
                .Build();

            Subject.Add(commandModel);
        }

        [Test]
        public void should_not_return_disk_access_command_if_another_running()
        {
            GivenStartedDiskCommand();

            var newCommandModel = Builder<CommandModel>
                .CreateNew()
                .With(c => c.Name = "ProcessMonitoredDownloads")
                .With(c => c.Body = new ProcessMonitoredDownloadsCommand())
                .Build();

            Subject.Add(newCommandModel);

            Subject.TryGet(out var command);

            command.Should().BeNull();
        }

        [Test]
        public void should_not_return_type_exclusive_command_if_another_running()
        {
            GivenStartedTypeExclusiveCommand();

            var newCommandModel = Builder<CommandModel>
                .CreateNew()
                .With(c => c.Name = "ImportListSync")
                .With(c => c.Body = new ImportListSyncCommand())
                .Build();

            Subject.Add(newCommandModel);

            Subject.TryGet(out var command);

            command.Should().BeNull();
        }

        [Test]
        public void should_not_return_type_exclusive_command_if_another_and_disk_access_command_running()
        {
            GivenStartedTypeExclusiveCommand();
            GivenStartedDiskCommand();

            var newCommandModel = Builder<CommandModel>
                .CreateNew()
                .With(c => c.Name = "ImportListSync")
                .With(c => c.Body = new ImportListSyncCommand())
                .Build();

            Subject.Add(newCommandModel);

            Subject.TryGet(out var command);

            command.Should().BeNull();
        }

        [Test]
        public void should_return_type_exclusive_command_if_another_not_running()
        {
            GivenStartedDiskCommand();

            var newCommandModel = Builder<CommandModel>
                .CreateNew()
                .With(c => c.Name = "ImportListSync")
                .With(c => c.Body = new ImportListSyncCommand())
                .Build();

            Subject.Add(newCommandModel);

            Subject.TryGet(out var command);

            command.Should().NotBeNull();
            command.Status.Should().Be(CommandStatus.Started);
        }

        [Test]
        public void should_return_regular_command_if_type_exclusive_command_running()
        {
            GivenStartedTypeExclusiveCommand();

            var newCommandModel = Builder<CommandModel>
                .CreateNew()
                .With(c => c.Name = "RefreshAuthor")
                .With(c => c.Body = new RefreshAuthorCommand())
                .Build();

            Subject.Add(newCommandModel);

            Subject.TryGet(out var command);

            command.Should().NotBeNull();
            command.Status.Should().Be(CommandStatus.Started);
        }

        [Test]
        public void should_not_return_exclusive_command_if_any_running()
        {
            GivenStartedDiskCommand();

            var newCommandModel = Builder<CommandModel>
                .CreateNew()
                .With(c => c.Name = "ApplicationUpdate")
                .With(c => c.Body = new ApplicationUpdateCommand())
                .Build();

            Subject.Add(newCommandModel);

            Subject.TryGet(out var command);

            command.Should().BeNull();
        }

        [Test]
        public void should_not_return_any_command_if_exclusive_running()
        {
            GivenStartedExclusiveCommand();

            var newCommandModel = Builder<CommandModel>
                .CreateNew()
                .With(c => c.Name = "RefreshAuthor")
                .With(c => c.Body = new RefreshAuthorCommand())
                .Build();

            Subject.Add(newCommandModel);

            Subject.TryGet(out var command);

            command.Should().BeNull();
        }

        [Test]
        public void should_return_null_if_nothing_queued()
        {
            GivenStartedDiskCommand();

            Subject.TryGet(out var command);

            command.Should().BeNull();
        }
    }
}
