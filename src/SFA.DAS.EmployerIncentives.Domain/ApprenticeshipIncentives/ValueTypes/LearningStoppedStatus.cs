using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class LearningStoppedStatus : ValueObject
    {
        public bool LearningStopped { get; }

        public DateTime? DateStopped { get; private set; }
        public DateTime? DateResumed { get; private set; }

        public LearningStoppedStatus(bool isStopped, DateTime? dateChanged = null)
        {
            if(isStopped && !dateChanged.HasValue)
            {
                throw new ArgumentException("Date changed must be provided when learning stopped");
            }
            LearningStopped = isStopped;
            if (isStopped)
            {
                DateStopped = dateChanged;
                DateResumed = null;
            }
            else 
            {
                DateResumed = dateChanged;
                DateStopped = null;
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return LearningStopped;
            yield return DateStopped;
            yield return DateResumed;
        }

        public void Undo()
        {
            DateStopped = null;
            DateResumed = null;
        }
    }
}
