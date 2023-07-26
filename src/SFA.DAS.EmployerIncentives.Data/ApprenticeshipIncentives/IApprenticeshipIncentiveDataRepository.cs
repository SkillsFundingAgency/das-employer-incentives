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
        Task Delete(ApprenticeshipIncentiveModel apprenticeshipIncentive);

        Task<ApprenticeshipIncentiveModel> FindByApprenticeshipId(Guid incentiveApplicationApprenticeshipId);
        Task<List<ApprenticeshipIncentiveModel>> FindApprenticeshipIncentivesWithoutPendingPayments(bool includeStopped = false, bool includeWithdrawn = false);
        Task<List<ApprenticeshipIncentiveModel>> FindApprenticeshipIncentiveByUlnWithinAccountLegalEntity(long uln, long accountLegalEntityId);
        Task<ApprenticeshipIncentiveModel> FindApprenticeshipIncentiveByEmploymentCheckId(Guid correlationId);
        Task<List<ApprenticeshipIncentiveModel>> FindIncentivesWithLearningFound();
        Task<List<ApprenticeshipIncentiveModel>> FindByAccountLegalEntityId(long accountLegalEntityId);
    }
}
