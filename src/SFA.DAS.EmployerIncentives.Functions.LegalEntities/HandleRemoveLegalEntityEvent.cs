using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerIncentives.Application.Commands;
using SFA.DAS.EmployerIncentives.Application.Commands.RemoveLegalEntity;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities
{
    public class HandleRemoveLegalEntityEvent
    {
        private readonly ICommandHandler<RemoveLegalEntityCommand> _handler;

        public HandleRemoveLegalEntityEvent(ICommandHandler<RemoveLegalEntityCommand> handler)
        {
            _handler = handler;
        }

        [FunctionName("HandleRemovedLegalEntityEvent")]
        public async Task Run([NServiceBusTrigger(Endpoint = QueueNames.RemovedLegalEntity)] RemovedLegalEntityEvent message)
        {
            await _handler.Handle(new RemoveLegalEntityCommand(
                message.AccountId,
                message.LegalEntityId,
                message.AccountLegalEntityId));
        }
    }
}
