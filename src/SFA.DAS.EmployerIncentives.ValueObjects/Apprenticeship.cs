using System;

namespace SFA.DAS.EmployerIncentives.ValueObjects
{
    public class Apprenticeship
    {
        public Apprenticeship(long uniqueLearnerNumber, string name, DateTime dateOfBirth, DateTime startDate, bool isApproved)
        {
            UniqueLearnerNumber = uniqueLearnerNumber;
            Name = name;
            DateOfBirth = dateOfBirth;
            StartDate = startDate;
            IsApproved = isApproved;
            //Course
            //ApprenticeshipEmployerTypeOnApproval
        }

        public long UniqueLearnerNumber { get; }
        public string Name { get; }
        public DateTime DateOfBirth { get; }
        public DateTime StartDate { get; }
        public bool IsApproved { get; }
    }
}
