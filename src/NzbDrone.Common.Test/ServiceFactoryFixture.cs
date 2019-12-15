using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using Radarr.Host;
using NzbDrone.Test.Common;
using NzbDrone.Core.CustomFormats;
using System.Collections.Generic;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class ServiceFactoryFixture : TestBase<ServiceFactory>
    {
        [Test]
        public void event_handlers_should_be_unique()
        {
            var container = MainAppContainerBuilder.BuildContainer(new StartupContext());
            container.Register<IMainDatabase>(new MainDatabase(null));
            container.Resolve<IAppFolderFactory>().Register();

            // A dummy custom format repository since this isn't a DB test
            var mockCustomFormat = Mocker.GetMock<ICustomFormatRepository>();
            mockCustomFormat.Setup(x => x.All()).Returns(new List<CustomFormatDefinition>());
            container.Register<ICustomFormatRepository>(mockCustomFormat.Object);

            Mocker.SetConstant(container);

            var handlers = Subject.BuildAll<IHandle<ApplicationStartedEvent>>()
                                  .Select(c => c.GetType().FullName);

            handlers.Should().OnlyHaveUniqueItems();
        }
    }
}
