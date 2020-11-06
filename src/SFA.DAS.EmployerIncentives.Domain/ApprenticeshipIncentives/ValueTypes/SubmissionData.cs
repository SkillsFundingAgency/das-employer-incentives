using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class SubmissionData : ValueObject
    {
        public SubmissionData(DateTime submissionDate, LearningFoundStatus learningFoundStatus)
        {
            SubmissionDate = submissionDate;
            LearningFoundStatus = learningFoundStatus;
        }

        public SubmissionData(DateTime submissionDate, bool learningFound)
        {
            SubmissionDate = submissionDate;
            LearningFoundStatus = new LearningFoundStatus(learningFound);
        }


        public DateTime SubmissionDate { get; }

        public LearningFoundStatus LearningFoundStatus { get; }

        public DateTime? StartDate { get; private set; }

        public string RawJson { get; private set; }

        public void SetStartDate(DateTime? startDate)
        {
            StartDate = startDate;
        }

        public void SetRawJson(string rawJson)
        {
            RawJson = rawJson;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return SubmissionDate;
            yield return LearningFoundStatus;
        }
    }
}
