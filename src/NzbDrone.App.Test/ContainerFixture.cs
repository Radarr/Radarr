﻿using System.Collections.Generic;
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
            var first = _container.ResolveAll<IHandle<EpisodeGrabbedEvent>>().OfType<DownloadMonitoringService>().Single();
            var second = _container.ResolveAll<IHandle<EpisodeGrabbedEvent>>().OfType<DownloadMonitoringService>().Single();

            first.Should().BeSameAs(second);
        }

        [Test]
        public void should_return_same_instance_of_singletons_by_different_interfaces()
        {
            var first = _container.ResolveAll<IHandle<EpisodeGrabbedEvent>>().OfType<DownloadMonitoringService>().Single();
            var second = (DownloadMonitoringService)_container.Resolve<IExecute<CheckForFinishedDownloadCommand>>();

            first.Should().BeSameAs(second);
        }
    }
}