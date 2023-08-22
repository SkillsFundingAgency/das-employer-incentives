using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public interface IApprenticeshipIncentiveDomainRepository : IDomainRepository<Guid, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>
    {
        Task<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> FindByApprenticeshipId(Guid incentiveApplicationApprenticeshipId);
        
        Task<List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>> FindIncentivesWithoutPendingPayments();

        Task<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> FindByUlnWithinAccountLegalEntity(long uln, long accountLegalEntityId);

        Task<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> FindByEmploymentCheckId(Guid correlationId);
        Task<List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>> FindIncentivesWithLearningFound();
        Task<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> FindByPaymentId(Guid paymentId);
        Task<List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>> FindByAccountLegalEntity(long accountLegalEntityId);
    }
}
