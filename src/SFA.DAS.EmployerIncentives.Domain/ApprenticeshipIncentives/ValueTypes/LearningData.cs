using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class LearningData : ValueObject, ILogWriter
    {
        public bool LearningFound { get; }
        public string NotFoundReason { get;}
        public bool? HasDataLock { get; private set; }
        public bool? IsInlearning { get; private set; }
        public DateTime? StartDate { get; private set; }
        public int? DaysinLearning { get; }
        public LearningStoppedStatus StoppedStatus { get; private set; }
        
        public LearningData(bool isFound, string notFoundReason = "")
        {
            LearningFound = isFound;
            NotFoundReason = notFoundReason;
            StoppedStatus = new LearningStoppedStatus(false);
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

        public void SetIsStopped(LearningStoppedStatus stoppedStatus)
        {
            StoppedStatus = LearningFound ? stoppedStatus : null;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return NotFoundReason;
            yield return HasDataLock;
            yield return IsInlearning;
            yield return StartDate;
            yield return DaysinLearning;
            yield return StoppedStatus;
        }

        public Log Log
        {
            get
            {

                return new Log
                {
                    OnProcessed = () => $"Learning data : LearningFound : {LearningFound}, StartDate : {StartDate}, HasDataLock : {HasDataLock}, IsInlearning : {IsInlearning}, DaysinLearning : {DaysinLearning}, StoppedStatus : {StoppedStatus}"
                };
            }
        }

    }
}
