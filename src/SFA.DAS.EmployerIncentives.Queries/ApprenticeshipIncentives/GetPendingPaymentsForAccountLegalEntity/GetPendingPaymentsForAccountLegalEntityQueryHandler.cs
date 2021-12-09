using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity
{
    public class GetPendingPaymentsForAccountLegalEntityQueryHandler : IQueryHandler<GetPendingPaymentsForAccountLegalEntityRequest, GetPendingPaymentsForAccountLegalEntityResponse>
    {
        private readonly IQueryRepository<PendingPaymentDto> _queryRepository;

        public GetPendingPaymentsForAccountLegalEntityQueryHandler(IQueryRepository<PendingPaymentDto> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<GetPendingPaymentsForAccountLegalEntityResponse> Handle(GetPendingPaymentsForAccountLegalEntityRequest query, CancellationToken cancellationToken = default)
        {
            var pendingPayments = await _queryRepository.GetList(dto =>
                dto.AccountLegalEntityId == query.AccountLegalEntityId && 
                !dto.PaymentMadeDate.HasValue &&
                (dto.PaymentYear < query.CollectionPeriod.AcademicYear || (dto.PaymentYear == query.CollectionPeriod.AcademicYear && dto.PeriodNumber <= query.CollectionPeriod.PeriodNumber)));

            return new GetPendingPaymentsForAccountLegalEntityResponse(pendingPayments);
        }
    }
}
