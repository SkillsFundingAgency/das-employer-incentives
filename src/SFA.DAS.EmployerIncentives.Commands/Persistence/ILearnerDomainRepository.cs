using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public interface ILearnerDomainRepository : IDomainRepository<Guid, Domain.ApprenticeshipIncentives.Learner>
    {
        Task<Domain.ApprenticeshipIncentives.Learner> GetByApprenticeshipIncentiveId(Guid incentiveId);
    }
}
