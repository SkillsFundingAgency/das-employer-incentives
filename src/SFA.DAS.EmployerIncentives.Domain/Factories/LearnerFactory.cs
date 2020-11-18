
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public class LearnerFactory : ILearnerFactory
    {
        public Learner CreateNew(Guid id, Guid applicationApprenticeshipId, long apprenticeshipId, long ukprn, long uniqueLearnerNumber, DateTime createdDate)
        {
            return Learner.New(id, applicationApprenticeshipId, apprenticeshipId, ukprn, uniqueLearnerNumber, createdDate);
        }

        public Learner GetExisting(LearnerModel model)
        {
            return Learner.Get(model);
        }
    }
}
