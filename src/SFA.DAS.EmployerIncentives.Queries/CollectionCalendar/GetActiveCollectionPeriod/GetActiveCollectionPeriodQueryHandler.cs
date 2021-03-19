using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.GetActiveCollectionPeriod
{
    public class GetActiveCollectionPeriodQueryHandler : IQueryHandler<GetActiveCollectionPeriodRequest, GetActiveCollectionPeriodResponse>
    {
        private readonly ICollectionPeriodDataRepository _queryRepository;

        public GetActiveCollectionPeriodQueryHandler(ICollectionPeriodDataRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<GetActiveCollectionPeriodResponse> Handle(GetActiveCollectionPeriodRequest query, CancellationToken cancellationToken = default)
        {
            var activePeriod = new Domain.ValueObjects.CollectionCalendar(await _queryRepository.GetAll()).GetActivePeriod();
            return new GetActiveCollectionPeriodResponse(new Abstractions.DTOs.Queries.ApprenticeshipIncentives.CollectionPeriodDto() 
            {
                CollectionPeriodNumber = activePeriod.PeriodNumber,
                CollectionYear = activePeriod.AcademicYear
            });
        }
    }
}
