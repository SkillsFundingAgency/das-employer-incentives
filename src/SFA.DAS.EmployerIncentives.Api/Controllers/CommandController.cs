using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ApiCommandControllerBase
    {
        public CommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher) { }
        const string typesPrefix = "SFA.DAS.EmployerIncentives.Commands.Types.";

        [HttpPost("/commands/{type}")]
        public async Task<IActionResult> RunCommand(string type, [FromBody] string commandText)
        {               
            var objectType = typeof(CreateCommand).Assembly.GetType($"{typesPrefix}{type}");
            var command = JsonConvert.DeserializeObject(commandText, objectType);
            
            await SendCommandAsync(command as dynamic);
            return Ok();
        }

    }
}
