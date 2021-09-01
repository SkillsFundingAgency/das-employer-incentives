using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IAcademicYearDataRepository
    {
        Task<IEnumerable<Domain.ValueObjects.AcademicYear>> GetAll();
    }
}
