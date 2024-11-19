using Radarr.Api.V3.Queue;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class QueueClient : ClientBase<QueueResource>
    {
        public QueueClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }
    }
}
