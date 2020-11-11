using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public class CollectionCalendarService : ICollectionCalendarService
    {
        private readonly ICollectionPeriodDataRepository _collectionPeriodDataRepository;

        public CollectionCalendarService(ICollectionPeriodDataRepository collectionPeriodDataRepository)
        {
            _collectionPeriodDataRepository = collectionPeriodDataRepository;
        }

        public async Task<CollectionCalendar> Get()
        {
            var periods = await _collectionPeriodDataRepository.GetAll();
            return new CollectionCalendar(periods);
        }
    }
}
