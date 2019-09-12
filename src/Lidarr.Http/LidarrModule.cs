using Nancy;
using Nancy.Responses.Negotiation;

namespace Lidarr.Http
{
    public abstract class LidarrModule : NancyModule
    {
        protected LidarrModule(string resource)
        : base(resource)
        {
        }

        protected Negotiator ResponseWithCode(object model, HttpStatusCode statusCode)
        {
            return Negotiate.WithModel(model).WithStatusCode(statusCode);
        }
    }
}
