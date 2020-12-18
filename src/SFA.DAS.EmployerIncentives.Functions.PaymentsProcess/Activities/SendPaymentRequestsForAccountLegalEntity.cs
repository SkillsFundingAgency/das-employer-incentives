using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class SendPaymentRequestsForAccountLegalEntity
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private ILogger<SendPaymentRequestsForAccountLegalEntity> _logger;

        public SendPaymentRequestsForAccountLegalEntity(ICommandDispatcher commandDispatcher, ILogger<SendPaymentRequestsForAccountLegalEntity> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName("SendPaymentRequestsForAccountLegalEntity")]
        public async Task<bool> Send([ActivityTrigger]AccountLegalEntityCollectionPeriod accountLegalEntityCollectionPeriod)
        {
            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;
            var accountLegalEntityId = accountLegalEntityCollectionPeriod.AccountLegalEntityId;
            _logger.LogInformation("Publish SendPaymentRequestsCommand for account legal entity {accountLegalEntityId}, collection period {collectionPeriod}", accountLegalEntityId, collectionPeriod);
            await _commandDispatcher.Send(new SendPaymentRequestsCommand(accountLegalEntityId, DateTime.UtcNow));
            _logger.LogInformation("Published SendPaymentRequestsCommand for account legal entity {accountLegalEntityId}, collection period {collectionPeriod}", accountLegalEntityId, collectionPeriod);
            return true;
        }
    }
}