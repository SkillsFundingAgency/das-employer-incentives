using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendClawbacks;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class SendClawbacksForAccountLegalEntity
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<SendClawbacksForAccountLegalEntity> _logger;

        public SendClawbacksForAccountLegalEntity(ICommandDispatcher commandDispatcher, ILogger<SendClawbacksForAccountLegalEntity> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(SendClawbacksForAccountLegalEntity))]
        public async Task<bool> Send([ActivityTrigger] AccountLegalEntityCollectionPeriod accountLegalEntityCollectionPeriod)
        {
            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;
            var accountLegalEntityId = accountLegalEntityCollectionPeriod.AccountLegalEntityId;
            _logger.LogInformation("[SendClawbacksForAccountLegalEntity] Publish SendClawbackRequestsCommand for account legal entity {accountLegalEntityId}, collection period {collectionPeriod}", accountLegalEntityId, collectionPeriod);
            await _commandDispatcher.Send(new SendClawbacksCommand(accountLegalEntityId, DateTime.UtcNow));
            _logger.LogInformation("[SendClawbacksForAccountLegalEntity] Published SendClawbacksCommand for account legal entity {accountLegalEntityId}, collection period {collectionPeriod}", accountLegalEntityId, collectionPeriod);
            return true;
        }
    }
}