using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class SubmissionData : ValueObject, ILogWriter
    {
        public bool SubmissionFound { get; private set; }
        public DateTime? SubmissionDate { get; private set; }
        public LearningData LearningData { get; private set; }

        public SubmissionData()
        {
            LearningData = new LearningData();
        }

        public void SetSubmissionDate(DateTime? submissionDate)
        {
            SubmissionDate = submissionDate;
            if (submissionDate.HasValue)
            {
                SubmissionFound = true;
            }
            else
            {
                SubmissionFound = false;
            }
        }

        //public bool? IsInlearning { get; private set; }
        //public int? DaysinLearning { get; private set; }

        

        //public LearningFoundStatus LearningFoundStatus { get; private set; }

        //public DateTime? StartDate { get; private set; }
        //public bool HasDataLock { get; private set; }

        public string RawJson { get; private set; }

        //public void SetStartDate(DateTime? startDate)
        //{
        //    StartDate = startDate;
        //}

        //public void SetLearningData(LearningData learningData)
        //{
        //    LearningData = learningData;
        //}

        //public void SetLearningFound(LearningFoundStatus learningFoundStatus)
        //{
        //    LearningFoundStatus = learningFoundStatus;
        //}

        //public void SetHasDataLock(bool hasDataLock)
        //{
        //    HasDataLock = hasDataLock;
        //}

        //public void SetIsInLearning(bool? isInLearning)
        //{
        //    IsInlearning = isInLearning;
        //}

        public void SetRawJson(string rawJson)
        {
            RawJson = rawJson;
        }
                
        public Log Log
        {
            get
            {

                return new Log
                {
                    // TODO : OnProcessed = () => $"Submission data : LearningFound {LearningFoundStatus?.LearningFound}, StartDate {StartDate}, IsInlearning {IsInlearning}, HasDataLock {HasDataLock} "
                };
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {

            yield return SubmissionDate;
            yield return SubmissionFound;
            //yield return LearningFoundStatus;
            //yield return IsInlearning;            
            //yield return HasDataLock;
            yield return LearningData;
        }
    }
}
