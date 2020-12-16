using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.HasPayments
{
    public class HasPaymentsQueryHandler : IQueryHandler<HasPaymentsRequest, HasPaymentsResponse>
    {
        private readonly IApprenticeshipIncentiveQueryRepository _queryRepository;

        public HasPaymentsQueryHandler(IApprenticeshipIncentiveQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<HasPaymentsResponse> Handle(HasPaymentsRequest query, CancellationToken cancellationToken = default)
        {
            var incentiveModel = await _queryRepository.Get((a =>
              a.AccountLegalEntityId == query.AccountLegalEntityId &&
              a.ULN == query.ULN), includePayments: true);

            if (incentiveModel != null && incentiveModel.Payments.Any())
            {
                return new HasPaymentsResponse(true);
            }

            return new HasPaymentsResponse(false);
        }
    }
}
