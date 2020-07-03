using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    public class SampleCommandController : ApiCommandControllerBase
    {
        public SampleCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
            // HTTP POST, PUT, DELETE methods only
        }
    }
}