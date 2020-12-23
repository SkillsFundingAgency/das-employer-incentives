using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
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

        public async Task<Domain.ValueObjects.CollectionCalendar> Get()
        {
            var periods = await _collectionPeriodDataRepository.GetAll();
            return new Domain.ValueObjects.CollectionCalendar(periods);
        }

        public async Task Save(Domain.ValueObjects.CollectionCalendar collectionCalendar)
        {
            await _collectionPeriodDataRepository.Save(collectionCalendar.GetAllPeriods());
        }
    }
}
