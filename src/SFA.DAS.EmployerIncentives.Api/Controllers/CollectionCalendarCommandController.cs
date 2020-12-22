using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("collectionPeriods")]
    [ApiController]
    public class CollectionCalendarCommandController : ApiCommandControllerBase
    {
        public CollectionCalendarCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPatch]
        [Route("")]
        public async Task UpdateCollectionPeriod(UpdateCollectionPeriodRequest request)
        {
            await SendCommandAsync(new UpdateCollectionPeriodCommand(request.PeriodNumber, 
                                                                     request.AcademicYear,
                                                                     request.Active));
        }
    
    }
}
