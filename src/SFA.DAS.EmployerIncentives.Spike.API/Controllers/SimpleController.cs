using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Spike.Api.Client.Types;
using SFA.DAS.EmployerIncentives.Spike.API.SampleDomain;

namespace SFA.DAS.EmployerIncentives.Spike.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimpleController : ControllerBase
    {
        private readonly ILogger<SimpleController> _logger;

        public SimpleController(ILogger<SimpleController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new SimpleResponse {SayHelloName = "Hello Unknown"});
        }

        [HttpPost]
        public IActionResult Post([FromBody] SimpleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                throw new DomainException("Name", "Please supply a name");
            }

            return Ok(new SimpleResponse { SayHelloName = "Hello " + request.Name});
        }
    }
}
