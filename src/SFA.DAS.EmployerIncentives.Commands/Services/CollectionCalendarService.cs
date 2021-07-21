using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
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
            var academicYears = _academicYearDataRepository.GetAll();
            var periods = _collectionPeriodDataRepository.GetAll();
            await Task.WhenAll(academicYears, periods);
            return new Domain.ValueObjects.CollectionCalendar(academicYears.Result, periods.Result);
        }

        public async Task Save(Domain.ValueObjects.CollectionCalendar collectionCalendar)
        {
            await _collectionPeriodDataRepository.Save(collectionCalendar.GetAllPeriods());
        }
    }
}
