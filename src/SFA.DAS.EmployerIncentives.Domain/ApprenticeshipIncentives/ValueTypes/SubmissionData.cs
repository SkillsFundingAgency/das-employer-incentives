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
            LearningData = new LearningData(false);
        }
        public void SetLearningData(LearningData learningData)
        {
            LearningData = learningData;
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

        public string RawJson { get; private set; }

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
                    OnProcessed = () => $"Submission data : SubmissionFound {SubmissionFound}, SubmissionDate {SubmissionDate}, {LearningData.Log.OnProcessed()} "
                };
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return SubmissionDate;
            yield return SubmissionFound;
            yield return LearningData;
        }

        public bool HasChangeOfCircumstances(SubmissionData submissionData)
        {
            if (submissionData == null)
            {
                return false;
            }

            return LearningData.HasDataLock == submissionData.LearningData.HasDataLock &&
                   LearningData.StartDate == submissionData.LearningData.StartDate &&
                   LearningData.IsInlearning == submissionData.LearningData.IsInlearning &&
                   LearningData.LearningFound == submissionData.LearningData.LearningFound;
        }
    }
}
