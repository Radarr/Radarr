using Nancy.Bootstrapper;

namespace Radarr.Http.Extensions.Pipelines
{
    public interface IRegisterNancyPipeline
    {
        int Order { get; }

        void Register(IPipelines pipelines);
    }
}