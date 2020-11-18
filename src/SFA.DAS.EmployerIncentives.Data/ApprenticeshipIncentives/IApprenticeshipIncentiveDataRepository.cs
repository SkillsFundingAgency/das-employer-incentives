using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IApprenticeshipIncentiveDataRepository
    {
        Task Add(ApprenticeshipIncentiveModel apprenticeshipIncentive);
        Task<ApprenticeshipIncentiveModel> Get(Guid id);
        Task Update(ApprenticeshipIncentiveModel apprenticeshipIncentive);
    }
}
