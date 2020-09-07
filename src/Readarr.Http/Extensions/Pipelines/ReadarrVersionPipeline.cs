using Nancy;
using Nancy.Bootstrapper;
using NzbDrone.Common.EnvironmentInfo;

namespace Readarr.Http.Extensions.Pipelines
{
    public class ReadarrVersionPipeline : IRegisterNancyPipeline
    {
        public int Order => 0;

        public void Register(IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToStartOfPipeline(Handle);
        }

        private void Handle(NancyContext context)
        {
            if (!context.Response.Headers.ContainsKey("X-ApplicationVersion"))
            {
                context.Response.Headers.Add("X-ApplicationVersion", BuildInfo.Version.ToString());
            }
        }
    }
}
