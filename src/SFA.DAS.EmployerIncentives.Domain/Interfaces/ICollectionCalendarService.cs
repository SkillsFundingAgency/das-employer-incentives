using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface ICollectionCalendarService
    {
        Task<Domain.ValueObjects.CollectionCalendar> Get();

        Task Save(Domain.ValueObjects.CollectionCalendar collectionCalendar);
    }
}
