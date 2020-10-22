using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class PendingPaymentValidationResult : ValueObject
    {
        public string ValidationStep { get; }
        public bool ValidationResult { get; }
        public CollectionPeriod CollectionPeriod { get; }

        public PendingPaymentValidationResult(
            string validationStep,
            bool validationResult,
            CollectionPeriod collectionPeriod)
        {
            ValidationStep = validationStep;
            ValidationResult = validationResult;
            CollectionPeriod = collectionPeriod;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ValidationStep;
            yield return ValidationResult;
            yield return CollectionPeriod;
        }
    }
}
