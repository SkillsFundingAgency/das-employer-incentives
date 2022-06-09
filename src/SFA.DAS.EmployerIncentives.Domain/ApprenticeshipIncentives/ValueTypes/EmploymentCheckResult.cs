using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class EmploymentCheckResult : ValueObject
    {
        public Guid CorrelationId { get; }
        public EmploymentCheckResultType Result { get; }
        public EmploymentCheckResultError? ErrorType { get; }
        public DateTime DateChecked { get; }

        public EmploymentCheckResult(Guid correlationId, EmploymentCheckResultType result, DateTime dateChecked, EmploymentCheckResultError? errorType = null)
        {
            CorrelationId = correlationId;
            Result = result;
            DateChecked = dateChecked;
            ErrorType = errorType;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return CorrelationId;
            yield return Result;
            yield return DateChecked;
            yield return ErrorType;
        }
    }
}
