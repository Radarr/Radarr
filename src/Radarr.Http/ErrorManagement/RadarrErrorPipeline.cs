using System.Data.SQLite;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using NLog;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Exceptions;
using Radarr.Http.Exceptions;

namespace Radarr.Http.ErrorManagement
{
    public class RadarrErrorPipeline
    {
        private readonly Logger _logger;

        public RadarrErrorPipeline(Logger logger)
        {
            _logger = logger;
        }

        public async Task HandleException(HttpContext context)
        {
            _logger.Trace("Handling Exception");

            var response = context.Response;
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            _logger.Warn(exception);

            var statusCode = HttpStatusCode.InternalServerError;
            var errorModel = new ErrorModel
            {
                Message = exception.Message,
                Description = exception.ToString()
            };

            if (exception is ApiException apiException)
            {
                _logger.Warn(apiException, "API Error:\n{0}", apiException.Message);

                /* var body = RequestStream.FromStream(context.Request.Body).AsString();
                 _logger.Trace("Request body:\n{0}", body);*/

                errorModel = new ErrorModel(apiException);
                statusCode = apiException.StatusCode;
            }
            else if (exception is ValidationException validationException)
            {
                _logger.Warn("Invalid request {0}", validationException.Message);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.ContentType = "application/json";
                await response.WriteAsync(STJson.ToJson(validationException.Errors));
                return;
            }
            else if (exception is NzbDroneClientException clientException)
            {
                errorModel = new ErrorModel
                {
                    Message = exception.Message,
                    Description = exception.ToString()
                };
                statusCode = clientException.StatusCode;
            }
            else if (exception is ModelNotFoundException notFoundException)
            {
                errorModel = new ErrorModel
                {
                    Message = exception.Message,
                    Description = exception.ToString()
                };
                statusCode = HttpStatusCode.NotFound;
            }
            else if (exception is ModelConflictException conflictException)
            {
                _logger.Error(exception, "DB error");
                errorModel = new ErrorModel
                {
                    Message = exception.Message,
                    Description = exception.ToString()
                };
                statusCode = HttpStatusCode.Conflict;
            }
            else if (exception is SQLiteException sqLiteException)
            {
                if (context.Request.Method == "PUT" || context.Request.Method == "POST")
                {
                    if (sqLiteException.Message.Contains("constraint failed"))
                    {
                        errorModel = new ErrorModel
                        {
                            Message = exception.Message,
                        };
                        statusCode = HttpStatusCode.Conflict;
                    }
                }

                _logger.Error(sqLiteException, "[{0} {1}]", context.Request.Method, context.Request.Path);
            }

            _logger.Fatal(exception, "Request Failed. {0} {1}", context.Request.Method, context.Request.Path);

            await errorModel.WriteToResponse(response, statusCode);
        }
    }
}
