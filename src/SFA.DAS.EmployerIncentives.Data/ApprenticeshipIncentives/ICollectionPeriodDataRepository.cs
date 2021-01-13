using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface ICollectionPeriodDataRepository
    {
        Task<IEnumerable<CollectionPeriod>> GetAll();
        Task Save(IEnumerable<CollectionPeriod> collectionPeriods);
    }
}
