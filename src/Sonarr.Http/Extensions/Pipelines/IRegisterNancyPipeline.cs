using Nancy.Bootstrapper;

namespace Lidarr.Http.Extensions.Pipelines
{
    public interface IRegisterNancyPipeline
    {
        int Order { get; }

        void Register(IPipelines pipelines);
    }
}