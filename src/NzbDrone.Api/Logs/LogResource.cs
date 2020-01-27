﻿using System;
using Radarr.Http.REST;

namespace NzbDrone.Api.Logs
{
    public class LogResource : RestResource
    {
        public DateTime Time { get; set; }
        public string Exception { get; set; }
        public string ExceptionType { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
    }

    public static class LogResourceMapper
    {
        public static LogResource ToResource(this Core.Instrumentation.Log model)
        {
            if (model == null)
            {
                return null;
            }

            return new LogResource
            {
                Id = model.Id,

                Time = model.Time,
                Exception = model.Exception,
                ExceptionType = model.ExceptionType,
                Level = model.Level,
                Logger = model.Logger,
                Message = model.Message
            };
        }
    }
}
