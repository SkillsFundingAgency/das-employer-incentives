using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities
{
    public class HandleRefreshLegalEntitiesRequest
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public HandleRefreshLegalEntitiesRequest(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }
        [FunctionName("HttpTriggerRefreshLegalEntities")]
        public Task Run([HttpTrigger(AuthorizationLevel.Function)] HttpRequest request)
        {
            return _commandDispatcher.Send(new RefreshLegalEntitiesCommand());
        }
    }
}
