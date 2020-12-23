using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class LearningData : ValueObject
    {        
        public bool LearningFound { get; private set; }
        public string NotFoundReason { get; private set; }
        public bool? HasDataLock { get; private set; }
        public bool? IsInlearning { get; private set; }
        public DateTime? StartDate { get; private set; }
        public int? DaysinLearning { get; private set; }

        public LearningData()
        {
        }

        public void SetLearningFound(bool isFound, string notFoundReason = "")
        {
            LearningFound = isFound;
            NotFoundReason = notFoundReason;
            if(isFound)
            {
                HasDataLock = null;
                IsInlearning = null;
                StartDate = null;
                DaysinLearning = null;
            }
        }

        public void SetHasDataLock(bool hasDataLock)
        {
            HasDataLock = LearningFound && hasDataLock;
        }

        public void SetStartDate(DateTime? startDate)
        {
            StartDate = LearningFound ? startDate : null;
        }

        public void SetIsInLearning(bool? isInLearning)
        {
            IsInlearning = LearningFound ? isInLearning : null;            
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return NotFoundReason;
            yield return HasDataLock;
            yield return IsInlearning;
            yield return StartDate;
            yield return DaysinLearning;
        }
    }

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
