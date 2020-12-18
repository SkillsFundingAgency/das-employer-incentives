using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.UlnHasPayments
{
    public class UlnHasPaymentsQueryHandler : IQueryHandler<UlnHasPaymentsRequest, UlnHasPaymentsResponse>
    {
        private readonly IApprenticeshipIncentiveQueryRepository _queryRepository;

        public UlnHasPaymentsQueryHandler(IApprenticeshipIncentiveQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<UlnHasPaymentsResponse> Handle(UlnHasPaymentsRequest query, CancellationToken cancellationToken = default)
        {
            var incentiveModel = await _queryRepository.Get((a =>
              a.AccountLegalEntityId == query.AccountLegalEntityId &&
              a.ULN == query.ULN), includePayments: true);

            if (incentiveModel != null && incentiveModel.Payments.Any())
            {
                return new UlnHasPaymentsResponse(true);
            }

            return new UlnHasPaymentsResponse(false);
        }
    }
}
