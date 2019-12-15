using System.Collections.Generic;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using Radarr.Host;
using NzbDrone.Test.Common;
using FluentAssertions;
using System.Linq;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.SignalR;
using Moq;
using NzbDrone.Core.CustomFormats;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class ContainerFixture : TestBase
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var args = new StartupContext("first", "second");

            _container = MainAppContainerBuilder.BuildContainer(args);

            _container.Register<IMainDatabase>(new MainDatabase(null));

            // set up a dummy broadcaster to allow tests to resolve
            var mockBroadcaster = new Mock<IBroadcastSignalRMessage>();
            _container.Register<IBroadcastSignalRMessage>(mockBroadcaster.Object);

            // A dummy custom format repository since this isn't a DB test
            var mockCustomFormat = Mocker.GetMock<ICustomFormatRepository>();
            mockCustomFormat.Setup(x => x.All()).Returns(new List<CustomFormatDefinition>());
            _container.Register<ICustomFormatRepository>(mockCustomFormat.Object);
        }

        [Test]
        public void should_be_able_to_resolve_indexers()
        {
            _container.Resolve<IEnumerable<IIndexer>>().Should().NotBeEmpty();
        }

        [Test]
        public void should_be_able_to_resolve_downloadclients()
        {
            _container.Resolve<IEnumerable<IDownloadClient>>().Should().NotBeEmpty();
        }

        [Test]
        public void container_should_inject_itself()
        {
            var factory = _container.Resolve<IServiceFactory>();

            factory.Build<IIndexerFactory>().Should().NotBeNull();
        }

        [Test]
        public void should_resolve_command_executor_by_name()
        {
            var genericExecutor = typeof(IExecute<>).MakeGenericType(typeof(RssSyncCommand));

            var executor = _container.Resolve(genericExecutor);

            executor.Should().NotBeNull();
            executor.Should().BeAssignableTo<IExecute<RssSyncCommand>>();
        }

        [Test]
		[Ignore("Shit appveyor")]
        public void should_return_same_instance_of_singletons()
        {
            var first = _container.ResolveAll<IHandle<ApplicationShutdownRequested>>().OfType<Scheduler>().Single();
            var second = _container.ResolveAll<IHandle<ApplicationShutdownRequested>>().OfType<Scheduler>().Single();

            first.Should().BeSameAs(second);
        }

        [Test]
        public void should_return_same_instance_of_singletons_by_different_same_interface()
        {
            var first = _container.ResolveAll<IHandle<MovieGrabbedEvent>>().OfType<DownloadMonitoringService>().Single();
            var second = _container.ResolveAll<IHandle<MovieGrabbedEvent>>().OfType<DownloadMonitoringService>().Single();

            first.Should().BeSameAs(second);
        }

        [Test]
        public void should_return_same_instance_of_singletons_by_different_interfaces()
        {
            var first = _container.ResolveAll<IHandle<MovieGrabbedEvent>>().OfType<DownloadMonitoringService>().Single();
            var second = (DownloadMonitoringService)_container.Resolve<IExecute<CheckForFinishedDownloadCommand>>();

            first.Should().BeSameAs(second);
        }
    }
}
