using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LegalEntityController : ControllerBase
    {

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
