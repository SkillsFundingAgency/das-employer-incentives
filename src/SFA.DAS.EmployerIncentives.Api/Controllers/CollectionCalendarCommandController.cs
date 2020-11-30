using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("collectionCalendar")]
    [ApiController]
    public class CollectionCalendarCommandController : ApiCommandControllerBase
    {
        public CollectionCalendarCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPatch]
        [Route("period/activate")]
        public async Task ActivateCollectionPeriod(ActivateCollectionPeriodRequest request)
        {
            await SendCommandAsync(new ActivateCollectionPeriodCommand(request.CollectionPeriodNumber, request.CollectionPeriodYear));
        }
    
    }
}
