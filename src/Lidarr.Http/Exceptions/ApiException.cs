using System;
using Nancy;
using Nancy.Responses;
using Lidarr.Http.ErrorManagement;
using Lidarr.Http.Extensions;
using Nancy.Configuration;

namespace Lidarr.Http.Exceptions
{
    public abstract class ApiException : Exception
    {
        public object Content { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        protected ApiException(HttpStatusCode statusCode, object content = null)
            : base(GetMessage(statusCode, content))
        {
            StatusCode = statusCode;
            Content = content;
        }

        public JsonResponse<ErrorModel> ToErrorResponse(NancyContext context)
        {
            return new ErrorModel(this).AsResponse(context, StatusCode);
        }

        private static string GetMessage(HttpStatusCode statusCode, object content)
        {
            var result = statusCode.ToString();

            if (content != null)
            {
                result = $"{result}: {content}";
            }

            return result;
        }
    }
}
