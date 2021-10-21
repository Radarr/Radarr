using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using NzbDrone.Common.Composition;

namespace NzbDrone.Host
{
    public class ControllerActivator : IControllerActivator
    {
        private readonly IContainer _container;

        public ControllerActivator(IContainer container)
        {
            _container = container;
        }

        public object Create(ControllerContext context)
        {
            return _container.Resolve(context.ActionDescriptor.ControllerTypeInfo.AsType());
        }

        public void Release(ControllerContext context, object controller)
        {
            // Nothing to do
        }
    }
}
