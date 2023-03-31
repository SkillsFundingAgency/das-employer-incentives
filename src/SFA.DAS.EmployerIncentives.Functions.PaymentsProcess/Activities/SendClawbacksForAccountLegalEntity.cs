using Microsoft.Azure.Functions.Worker;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendClawbacks;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class SendClawbacksForAccountLegalEntity
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public SendClawbacksForAccountLegalEntity(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;            
        }

        [Function(nameof(SendClawbacksForAccountLegalEntity))]
        public async Task<bool> Send([ActivityTrigger] AccountLegalEntityCollectionPeriod accountLegalEntityCollectionPeriod)
        {
            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;
            var accountLegalEntityId = accountLegalEntityCollectionPeriod.AccountLegalEntityId;
            await _commandDispatcher.Send(new SendClawbacksCommand(accountLegalEntityId, DateTime.UtcNow, new Domain.ValueObjects.CollectionPeriod(collectionPeriod.Period, collectionPeriod.Year)));
            return true;
        }
    }
}