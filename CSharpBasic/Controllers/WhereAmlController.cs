using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CSharpBasic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhereAmlController : ControllerBase
    {
        private readonly ILogger<WhereAmlController> _logger;

        public WhereAmlController(ILogger<WhereAmlController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public object Get()
        {
            _logger.LogInformation($"hello world");
            return HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}