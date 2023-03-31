using Microsoft.Azure.Functions.Worker;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class SendPaymentRequestsForAccountLegalEntity
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public SendPaymentRequestsForAccountLegalEntity(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [Function(nameof(SendPaymentRequestsForAccountLegalEntity))]
        public async Task<bool> Send([ActivityTrigger] AccountLegalEntityCollectionPeriod accountLegalEntityCollectionPeriod)
        {
            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;
            var accountLegalEntityId = accountLegalEntityCollectionPeriod.AccountLegalEntityId;
            await _commandDispatcher.Send(new SendPaymentRequestsCommand(accountLegalEntityId, DateTime.UtcNow, new Domain.ValueObjects.CollectionPeriod(collectionPeriod.Period, collectionPeriod.Year)));
            return true;
        }
    }
}