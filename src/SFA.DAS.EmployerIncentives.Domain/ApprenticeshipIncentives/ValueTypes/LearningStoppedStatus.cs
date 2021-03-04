using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class LearningStoppedStatus : ValueObject
    {
        public bool LearningStopped { get; }

        public DateTime? DateStopped { get; }

        public LearningStoppedStatus(bool isStopped, DateTime? dateStopped = null)
        {
            if(isStopped && !dateStopped.HasValue)
            {
                throw new ArgumentException("Date stopped mus be provided when leanring stopped");
            }
            LearningStopped = isStopped;
            DateStopped = dateStopped;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return LearningStopped;
            yield return DateStopped;
        }
    }
}
