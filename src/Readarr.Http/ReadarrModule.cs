using Nancy;
using Nancy.Responses.Negotiation;

namespace Readarr.Http
{
    public abstract class ReadarrModule : NancyModule
    {
        protected ReadarrModule(string resource)
        : base(resource)
        {
        }

        protected Negotiator ResponseWithCode(object model, HttpStatusCode statusCode)
        {
            return Negotiate.WithModel(model).WithStatusCode(statusCode);
        }
    }
}
