using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LegalEntityController : ControllerBase
    {
        private readonly ILogger<LegalEntityController> _logger;

        public LegalEntityController(ILogger<LegalEntityController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public Task<IActionResult> Add()
        {
           throw new NotImplementedException();
        }

        [HttpDelete]
        public Task<IActionResult> Remove()
        {
            throw new NotImplementedException();
        }
    }
}
