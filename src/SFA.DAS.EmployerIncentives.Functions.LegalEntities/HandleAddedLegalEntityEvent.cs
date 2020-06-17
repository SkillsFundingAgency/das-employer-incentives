using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerIncentives.Application.Commands;
using SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities
{
    public class HandleAddedLegalEntityEvent
    {
        private readonly ICommandHandler<AddLegalEntityCommand> _handler;

        public HandleAddedLegalEntityEvent(ICommandHandler<AddLegalEntityCommand> handler)
        {
            _handler = handler;
        }

        [FunctionName("HandleAddedLegalEntityEvent")]
        public async Task Run([NServiceBusTrigger(Endpoint = QueueNames.LegalEntityAdded)] AddedLegalEntityEvent message)
        {
            await _handler.Handle(new AddLegalEntityCommand(message.LegalEntityId));
        }
    }
}
