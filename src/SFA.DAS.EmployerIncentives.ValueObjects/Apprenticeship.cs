using System;

namespace SFA.DAS.EmployerIncentives.ValueObjects
{
    public class Apprenticeship
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
    }
}
