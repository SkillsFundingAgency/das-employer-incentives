using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class LearningFoundStatus : ValueObject
    {
        public bool LearningFound { get; }

        public string NotFoundReason { get; }

        public LearningFoundStatus(string learningNotFoundReason = null)
        {
            NotFoundReason = learningNotFoundReason;
            LearningFound = learningNotFoundReason == null;
        }

        public LearningFoundStatus(bool learningFound)
        {
            LearningFound = learningFound;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return LearningFound;
            yield return NotFoundReason;
        }
    }
}
