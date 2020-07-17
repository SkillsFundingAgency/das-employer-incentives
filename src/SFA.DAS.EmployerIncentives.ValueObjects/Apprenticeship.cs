using System;

namespace SFA.DAS.EmployerIncentives.ValueObjects
{
    public class Apprenticeship
    {
        public Apprenticeship(long uniqueLearnerNumber, string firstName, string lastName, DateTime dateOfBirth, DateTime startDate, bool isApproved)
        {
            UniqueLearnerNumber = uniqueLearnerNumber;
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            StartDate = startDate;
            IsApproved = isApproved;
            //Course
            //ApprenticeshipEmployerTypeOnApproval
        }

        public long UniqueLearnerNumber { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime DateOfBirth { get; }
        public DateTime StartDate { get; }
        public bool IsApproved { get; }
    }
}
