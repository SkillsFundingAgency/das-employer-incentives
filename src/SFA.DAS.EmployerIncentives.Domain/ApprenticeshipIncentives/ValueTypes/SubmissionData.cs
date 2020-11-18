﻿using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class SubmissionData : ValueObject, ILogWriter
    {
        public SubmissionData(DateTime submissionDate, bool learningFound)
        {
            SubmissionDate = submissionDate;
            LearningFound = learningFound;
        }

        public DateTime SubmissionDate { get; }
        public bool LearningFound { get; }
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
                    OnProcessed = () => $"Submisison data : LearningFound {LearningFound}"
                };
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return SubmissionDate;
            yield return LearningFound;
        }
    }
}
