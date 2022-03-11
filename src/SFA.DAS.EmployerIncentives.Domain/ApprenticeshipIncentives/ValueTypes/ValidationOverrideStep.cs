using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    [ExcludeFromCodeCoverage]
    public class ValidationOverrideStep : ValueObject
    {
        public ValidationOverrideStep(string validationType, DateTime expiryDate, bool remove = false)
        {
            ValidationType = validationType;
            ExpiryDate = expiryDate;
            Remove = remove;
        }

        public string ValidationType { get; }
        public DateTime ExpiryDate { get; }
        public bool Remove { get; }
        public bool HasExpired => ExpiryDate.Date <= DateTime.Today;

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ValidationType;
            yield return ExpiryDate;
            yield return Remove;
        }
    }
}
