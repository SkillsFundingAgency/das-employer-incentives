using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class GetPendingPaymentsForAccountLegalEntity
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private ILogger<GetPendingPaymentsForAccountLegalEntity> _logger;

        public GetPendingPaymentsForAccountLegalEntity(IQueryDispatcher queryDispatcher, ILogger<GetPendingPaymentsForAccountLegalEntity> logger)
        {
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(GetPendingPaymentsForAccountLegalEntity))]
        public async Task<List<PendingPaymentActivityDto>> Get([ActivityTrigger]AccountLegalEntityCollectionPeriod accountLegalEntityCollectionPeriod)
        {
            var accountLegalEntityId = accountLegalEntityCollectionPeriod.AccountLegalEntityId;
            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;
            _logger.LogInformation($"Calculate Payments process started for account legal entity {accountLegalEntityId}, collection period {collectionPeriod}", new { accountLegalEntityId, collectionPeriod });

            var request = new GetPendingPaymentsForAccountLegalEntityRequest(accountLegalEntityId, collectionPeriod.Year, collectionPeriod.Period);
            var pendingPayments = await _queryDispatcher.Send<GetPendingPaymentsForAccountLegalEntityRequest, GetPendingPaymentsForAccountLegalEntityResponse>(request);
            _logger.LogInformation($"{pendingPayments.PendingPayments.Count} pending payments returned for account legal entity {accountLegalEntityId}, collection period {collectionPeriod}", new { accountLegalEntityId, collectionPeriod });
            return pendingPayments.PendingPayments.Select(x => new PendingPaymentActivityDto { PendingPaymentId = x.Id, ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId }).ToList();
        }
    }
}
