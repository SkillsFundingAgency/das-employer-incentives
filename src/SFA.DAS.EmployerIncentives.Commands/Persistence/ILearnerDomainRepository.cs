using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public interface ILearnerDomainRepository : IDomainRepository<Guid, Learner>
    {
        Task<Learner> GetOrCreate(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive);
    }
}
