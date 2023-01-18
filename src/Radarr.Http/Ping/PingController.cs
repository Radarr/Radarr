using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.Ping
{
    public class PingController : Controller
    {
        private readonly IConfigRepository _configRepository;

        public PingController(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        [HttpGet("/ping")]
        [Produces("application/json")]
        public ActionResult<PingResource> GetStatus()
        {
            try
            {
                _configRepository.All();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new PingResource
                {
                    Status = "Error"
                });
            }

            return StatusCode(StatusCodes.Status200OK, new PingResource
            {
                Status = "OK"
            });
        }
    }
}
