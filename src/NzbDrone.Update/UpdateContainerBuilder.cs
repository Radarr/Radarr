using System.Collections.Generic;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Update
{
    public class UpdateContainerBuilder : ContainerBuilderBase
    {
        private UpdateContainerBuilder(IStartupContext startupContext, List<string> assemblies)
            : base(startupContext, assemblies)
        {
        }

        public static IContainer Build(IStartupContext startupContext)
        {
            var assemblies = new List<string>
                             {
                                 "Radarr.Update"
                             };

            return new UpdateContainerBuilder(startupContext, assemblies).Container;
        }
    }
}
