using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    [ExcludeFromCodeCoverage]
    public class ValidationOverrideStepAudit : ValueObject
    {
        public ValidationOverrideStepAudit(
            Guid id,
            Guid apprenticeshipIncentiveId,
            ValidationOverrideStep validationOverrideStep,
            ServiceRequest serviceRequest)
        {
            Id = id;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            ValidationOverrideStep = validationOverrideStep;
            ServiceRequest = serviceRequest;
        }

        public Guid Id { get; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public ValidationOverrideStep ValidationOverrideStep { get; }
        public ServiceRequest ServiceRequest { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return ApprenticeshipIncentiveId;
            yield return ValidationOverrideStep;
            yield return ServiceRequest;
        }
    }
}
