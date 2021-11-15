using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class SendPaymentRequestsForAccountLegalEntity
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<SendPaymentRequestsForAccountLegalEntity> _logger;

        public SendPaymentRequestsForAccountLegalEntity(ICommandDispatcher commandDispatcher, ILogger<SendPaymentRequestsForAccountLegalEntity> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(SendPaymentRequestsForAccountLegalEntity))]
        public async Task<bool> Send([ActivityTrigger] AccountLegalEntityCollectionPeriod accountLegalEntityCollectionPeriod)
        {
            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;
            var accountLegalEntityId = accountLegalEntityCollectionPeriod.AccountLegalEntityId;
            _logger.LogDebug("[SendPaymentRequestsForAccountLegalEntity] Publish SendPaymentRequestsCommand for account legal entity {accountLegalEntityId}, collection period {collectionPeriod}", accountLegalEntityId, collectionPeriod);
            await _commandDispatcher.Send(new SendPaymentRequestsCommand(accountLegalEntityId, DateTime.UtcNow));
            _logger.LogDebug("[SendPaymentRequestsForAccountLegalEntity] Published SendPaymentRequestsCommand for account legal entity {accountLegalEntityId}, collection period {collectionPeriod}", accountLegalEntityId, collectionPeriod);
            return true;
        }
    }
}