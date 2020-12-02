using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public interface ILearnerFactory
    {
        Learner CreateNew(Guid id, Guid applicationApprenticeshipId, long apprenticeshipId, long ukprn, long uniqueLearnerNumber);
        Learner GetExisting(LearnerModel model);
    }
}
