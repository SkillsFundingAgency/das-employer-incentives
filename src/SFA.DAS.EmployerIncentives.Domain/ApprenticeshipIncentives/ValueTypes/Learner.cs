using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class Learner : ValueObject, ILogWriter
    {
        public Learner(
            Guid id, 
            Guid apprenticeshipIncentiveId,
            long apprenticeshipId,
            long ukprn,
            long uniqueLearnerNumber,
            DateTime createdDate)
        {
            Id = id;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            ApprenticeshipId = apprenticeshipId;
            Ukprn = ukprn;
            UniqueLearnerNumber = uniqueLearnerNumber;
            CreatedDate = createdDate;
        }

        public Guid Id { get; }
        public Guid ApprenticeshipIncentiveId { get; }
        public long ApprenticeshipId { get; }
        public long Ukprn { get; }
        public long UniqueLearnerNumber { get; }
        public DateTime CreatedDate { get; }

        public bool SubmissionFound => SubmissionData != null;

        public SubmissionData SubmissionData { get; private set; }

        public void SetSubmissionData(SubmissionData submissionData)
        {
            SubmissionData = submissionData;
        }

        public Log Log
        {
            get
            {
                var message = $"Learner data : ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} and ApprenticeshipId {ApprenticeshipId} with Ukprn {Ukprn} and UniqueLearnerNumber {UniqueLearnerNumber}. ";

                if(SubmissionFound)
                {
                    message += ((ILogWriter)SubmissionData).Log.OnProcessed;
                }
                else
                {
                    message += "Submission data not found.";
                }

                return new Log
                {
                    OnProcessing = () => message,
                    OnProcessed = () => message,
                    OnError = () => message
                };
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return ApprenticeshipIncentiveId;
            yield return ApprenticeshipId;
            yield return Ukprn;
            yield return UniqueLearnerNumber;
            yield return CreatedDate;
            yield return SubmissionData;
        }
    }
}