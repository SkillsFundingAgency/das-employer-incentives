using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ApiCommandControllerBase
    {
        public CommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher) { }
        private const string TypesPrefix = "SFA.DAS.EmployerIncentives.Commands.Types.";

        [HttpPost("/commands/{type}")]
        public async Task<IActionResult> RunCommand(string type, [FromBody] string commandText)
        {
            var objectType = typeof(CreateApprenticeshipIncentiveCommand).Assembly.GetType($"{TypesPrefix}{type}");
            var command = JsonConvert.DeserializeObject(commandText, objectType);

            if (objectType != null && objectType.IsSubclassOf(typeof(DomainCommand)))
            {
                await SendCommandAsync((dynamic)command);
            }
            return Ok();
        }

    }
}
