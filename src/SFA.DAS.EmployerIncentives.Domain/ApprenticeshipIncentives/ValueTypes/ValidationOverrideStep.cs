using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class ValidationOverrideStep : ValueObject
    {
        public ValidationOverrideStep(string validationType, DateTime expiryDate)
        {
            ValidationType = validationType;
            ExpiryDate = expiryDate;
        }

        public string ValidationType { get; }
        public DateTime ExpiryDate { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ValidationType;
            yield return ExpiryDate;
        }
    }
}
