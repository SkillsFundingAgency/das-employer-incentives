using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public class CollectionCalendarService : ICollectionCalendarService
    {
        private readonly ICollectionPeriodDataRepository _collectionPeriodDataRepository;
        private readonly IAcademicYearDataRepository _academicYearDataRepository;

        public CollectionCalendarService(ICollectionPeriodDataRepository collectionPeriodDataRepository, IAcademicYearDataRepository academicYearDataRepository)
        {
            _collectionPeriodDataRepository = collectionPeriodDataRepository;
            _academicYearDataRepository = academicYearDataRepository;
        }

        public async Task<Domain.ValueObjects.CollectionCalendar> Get()
        {
            var academicYears = await _academicYearDataRepository.GetAll();
            var periods = await _collectionPeriodDataRepository.GetAll();
            return new Domain.ValueObjects.CollectionCalendar(academicYears, periods);
        }

        public async Task Save(Domain.ValueObjects.CollectionCalendar collectionCalendar)
        {
            await _collectionPeriodDataRepository.Save(collectionCalendar.GetAllPeriods());
        }
    }
}
