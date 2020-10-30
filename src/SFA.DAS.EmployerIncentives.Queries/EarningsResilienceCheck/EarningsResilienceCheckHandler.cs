using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.EarningsResilienceCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck
{
    public class EarningsResilienceCheckHandler : IQueryHandler<EarningsResilienceCheckRequest, EarningsResilienceCheckResponse>
    {
        private readonly IEarningsResilienceCheckRepository _repository;

        public EarningsResilienceCheckHandler(IEarningsResilienceCheckRepository repository)
        {
            _repository = repository;
        }

        public async Task<EarningsResilienceCheckResponse> Handle(EarningsResilienceCheckRequest query, CancellationToken cancellationToken = default)
        {
            var applications = await _repository.GetApplicationsWithoutEarningsCalculations();

            return new EarningsResilienceCheckResponse { Applications = applications };
        }
    }
}
