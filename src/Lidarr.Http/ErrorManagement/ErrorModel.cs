using Lidarr.Http.Exceptions;

namespace Lidarr.Http.ErrorManagement
{
    public class ErrorModel
    {
        public string Message { get; set; }
        public string Description { get; set; }
        public object Content { get; set; }

        public ErrorModel(ApiException exception)
        {
            Message = exception.Message;
            Content = exception.Content;
        }

        public ErrorModel()
        {
        }
    }
}