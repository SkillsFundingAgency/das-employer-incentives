using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IApprenticeshipIncentiveDataRepository
    {
        Task Add(ApprenticeshipIncentiveModel apprenticeshipIncentive);
        Task<ApprenticeshipIncentiveModel> Get(Guid id);
        Task Update(ApprenticeshipIncentiveModel apprenticeshipIncentive);

        Task<ApprenticeshipIncentiveModel> FindByApprenticeshipId(Guid incentiveApplicationApprenticeshipId);
    }
}
