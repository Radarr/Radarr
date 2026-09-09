using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NUnit.Framework;
using Radarr.Http;

namespace NzbDrone.Api.Test.ApplicationModels
{
    [TestFixture]
    public class ProducesResponseTypeConventionFixture
    {
        private sealed class SampleResource
        {
        }

        private sealed class SampleController
        {
            [ProducesResponseType(StatusCodes.Status202Accepted)]
            public ActionResult<SampleResource> Untyped() => null;

            [ProducesResponseType(typeof(SampleResource), StatusCodes.Status202Accepted)]
            public ActionResult<List<SampleResource>> AlreadyTyped() => null;

            [ProducesResponseType(StatusCodes.Status202Accepted)]
            public IActionResult NotAnActionResultOfT() => null;
        }

        private static ProducesResponseTypeAttribute Apply(string methodName)
        {
            var method = typeof(SampleController).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            var attribute = method.GetCustomAttribute<ProducesResponseTypeAttribute>();

            var action = new ActionModel(method, new List<object> { attribute });
            action.Filters.Add(attribute);

            new ProducesResponseTypeConvention().Apply(action);

            return attribute;
        }

        [Test]
        public void should_fill_response_type_in_from_the_action_result_argument()
        {
            Apply(nameof(SampleController.Untyped)).Type.Should().Be(typeof(SampleResource));
        }

        [Test]
        public void should_not_overwrite_an_explicitly_declared_response_type()
        {
            Apply(nameof(SampleController.AlreadyTyped)).Type.Should().Be(typeof(SampleResource));
        }

        [Test]
        public void should_leave_actions_that_do_not_return_action_result_of_t_alone()
        {
            Apply(nameof(SampleController.NotAnActionResultOfT)).Type.Should().Be(typeof(void));
        }
    }
}
