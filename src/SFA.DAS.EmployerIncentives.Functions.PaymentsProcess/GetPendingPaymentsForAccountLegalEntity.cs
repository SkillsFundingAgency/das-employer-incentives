using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
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

        [FunctionName("GetPendingPaymentsForAccountLegalEntity")]
        public async Task<List<Guid>> Get([ActivityTrigger]AccountLegalEntityCollectionPeriod accountLegalEntityCollectionPeriod)
        {
            var request = new GetPendingPaymentsForAccountLegalEntityRequest(accountLegalEntityCollectionPeriod.AccountLegalEntityId, accountLegalEntityCollectionPeriod.CollectionPeriod.Year, accountLegalEntityCollectionPeriod.CollectionPeriod.Month);
            var pendingPayments = await _queryDispatcher.Send<GetPendingPaymentsForAccountLegalEntityRequest, GetPendingPaymentsForAccountLegalEntityResponse>(request);
            _logger.LogInformation($"{pendingPayments.PendingPayments.Count} pending payments returned.");
            return pendingPayments.PendingPayments.Select(x => x.Id).ToList();
        }
    }
}
