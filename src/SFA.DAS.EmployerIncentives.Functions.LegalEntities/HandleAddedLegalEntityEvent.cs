using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.AddLegalEntity;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities
{
    public class HandleAddedLegalEntityEvent
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public HandleAddedLegalEntityEvent(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName("HandleAddedLegalEntityEvent")]
        public Task Run([NServiceBusTrigger(Endpoint = QueueNames.LegalEntityAdded)] AddedLegalEntityEvent message)
        {
            return _commandDispatcher.Send(
                new AddLegalEntityCommand(
                    message.AccountId,
                    message.LegalEntityId,
                    message.OrganisationName, 
                    message.AccountLegalEntityId)
                );
        }
    }
}
