using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface ICollectionCalendarService
    {
        Task<ValueObjects.CollectionCalendar> Get();

        Task Save(ValueObjects.CollectionCalendar collectionCalendar);
    }
}
