﻿using System;
using System.Data.SQLite;
using FluentValidation;
using Nancy;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Exceptions;
using Radarr.Http.Exceptions;
using Radarr.Http.Extensions;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace Radarr.Http.ErrorManagement
{
    public class RadarrErrorPipeline
    {
        private readonly Logger _logger;

        public RadarrErrorPipeline(Logger logger)
        {
            _logger = logger;
        }

        public Response HandleException(NancyContext context, Exception exception)
        {
            _logger.Trace("Handling Exception");

            if (exception is ApiException apiException)
            {
                _logger.Warn(apiException, "API Error");
                return apiException.ToErrorResponse();
            }
            
            if (exception is ValidationException validationException)
            {
                _logger.Warn("Invalid request {0}", validationException.Message);

                return validationException.Errors.AsResponse(HttpStatusCode.BadRequest);
            }

            if (exception is NzbDroneClientException clientException)
            {
                return new ErrorModel
                {
                    Message = exception.Message,
                    Description = exception.ToString()
                }.AsResponse((HttpStatusCode)clientException.StatusCode);
            }

            if (exception is ModelNotFoundException notFoundException)
            {
                return new ErrorModel
                {
                    Message = exception.Message,
                    Description = exception.ToString()
                }.AsResponse(HttpStatusCode.NotFound);
            }

            if (exception is ModelConflictException conflictException)
            {
                return new ErrorModel
                {
                    Message = exception.Message,
                    Description = exception.ToString()
                }.AsResponse(HttpStatusCode.Conflict);
            }

            if (exception is SQLiteException sqLiteException)
            {
                if (context.Request.Method == "PUT" || context.Request.Method == "POST")
                {
                    if (sqLiteException.Message.Contains("constraint failed"))
                        return new ErrorModel
                        {
                            Message = exception.Message,
                        }.AsResponse(HttpStatusCode.Conflict);
                }

                _logger.Error(sqLiteException, "[{0} {1}]", context.Request.Method, context.Request.Path);
            }

            _logger.Fatal(exception, "Request Failed. {0} {1}", context.Request.Method, context.Request.Path);

            return new ErrorModel
            {
                Message = exception.Message,
                Description = exception.ToString()
            }.AsResponse(HttpStatusCode.InternalServerError);
        }
    }
}