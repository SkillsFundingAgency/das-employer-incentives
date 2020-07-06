using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.AddLegalEntity;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.EmployerIncentives.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities
{
    public class HandleRefreshLegalEntityEvent
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public HandleRefreshLegalEntityEvent(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName("HandleRefreshLegalEntityEvent")]
        public async Task Run([NServiceBusTrigger(Endpoint = QueueNames.RefreshLegalEntityAdded)] RefreshLegalEntityEvent message)
        {
            await _commandDispatcher.Send(
                new AddLegalEntityCommand(
                    message.AccountId,
                    message.LegalEntityId,
                    message.OrganisationName,
                    message.AccountLegalEntityId)
                );
        }
    }
}
