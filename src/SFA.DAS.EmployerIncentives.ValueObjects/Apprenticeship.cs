using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.ValueObjects
{
    public class Apprenticeship : ValueObject
    {
        public Apprenticeship(long uniqueLearnerNumber, DateTime startDate, bool isApproved)
        {
            UniqueLearnerNumber = uniqueLearnerNumber;
            StartDate = startDate;
            IsApproved = isApproved;
        }

        public long UniqueLearnerNumber { get; }
        public DateTime StartDate { get; }
        public bool IsApproved { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return UniqueLearnerNumber;
            yield return StartDate;
            yield return IsApproved;
        }
    }
}
