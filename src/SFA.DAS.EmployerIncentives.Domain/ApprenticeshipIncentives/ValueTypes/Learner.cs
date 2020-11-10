using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class Learner : ValueObject // TODO: Turn into Entity
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

        public NextPendingPayment NextPendingPayment { get; private set; }


        public void SetNextPendingPayment(NextPendingPayment value)
        {
            NextPendingPayment = value;
        }

        public bool HasDataLock { get; private set; }

        public void SetHasDataLock() => HasDataLock = true;

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