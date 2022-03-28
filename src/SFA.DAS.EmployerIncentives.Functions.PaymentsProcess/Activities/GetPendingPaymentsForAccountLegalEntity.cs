using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.DataTransferObjects;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class GetPendingPaymentsForAccountLegalEntity
    {
        private readonly IQueryDispatcher _queryDispatcher;        

        public GetPendingPaymentsForAccountLegalEntity(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [FunctionName(nameof(GetPendingPaymentsForAccountLegalEntity))]
        public async Task<List<PendingPaymentActivityDto>> Get([ActivityTrigger]AccountLegalEntityCollectionPeriod accountLegalEntityCollectionPeriod)
        {
            var accountLegalEntityId = accountLegalEntityCollectionPeriod.AccountLegalEntityId;
            var collectionPeriod = accountLegalEntityCollectionPeriod.CollectionPeriod;

            var request = new GetPendingPaymentsForAccountLegalEntityRequest(accountLegalEntityId, new Domain.ValueObjects.CollectionPeriod(collectionPeriod.Period, collectionPeriod.Year));
            var pendingPayments = await _queryDispatcher.Send<GetPendingPaymentsForAccountLegalEntityRequest, GetPendingPaymentsForAccountLegalEntityResponse>(request);
            return pendingPayments.PendingPayments.Select(x => new PendingPaymentActivityDto { PendingPaymentId = x.Id, ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId }).ToList();
        }
    }
}
