using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class Apprenticeship : ValueObject
    {
        public long Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime DateOfBirth { get; }
        public long UniqueLearnerNumber { get; }
        public Provider Provider { get; private set; }
        public ApprenticeshipEmployerType EmployerType { get; }
        public string CourseName { get; private set; }
        public DateTime? EmploymentStartDate { get; private set; }

        public Apprenticeship(
            long id, 
            string firstName,
            string lastName,
            DateTime dateOfBirth,
            long uniqueLearnerNumber,
            ApprenticeshipEmployerType employerType,
            string courseName,
            DateTime? employmentStartDate)
        {
            if (id <= 0) throw new ArgumentException("Apprenticeship Id must be greater than 0", nameof(id));
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("FirstName must be set", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("LastName must be set", nameof(lastName));
            if (uniqueLearnerNumber <= 0) throw new ArgumentException("UniqueLearnerNumber must be greater than 0", nameof(uniqueLearnerNumber));
            if (string.IsNullOrWhiteSpace(courseName)) throw new ArgumentException("CourseName must be set", nameof(courseName));
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            UniqueLearnerNumber = uniqueLearnerNumber;
            EmployerType = employerType;
            CourseName = courseName;
            EmploymentStartDate = employmentStartDate;
        }

        public void SetProvider(Provider provider)
        {
            Provider = provider;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return FirstName;
            yield return LastName;
            yield return DateOfBirth;
            yield return UniqueLearnerNumber;
            yield return EmployerType;
            yield return Provider;
            yield return CourseName;
            yield return EmploymentStartDate;
        }
    }
}
