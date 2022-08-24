using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class EmploymentCheckRequestAudit : ValueObject
    {
        public Guid Id { get; }
        public Guid ApprenticeshipIncentiveId { get; set; }

        public EmploymentCheckType CheckType { get; }

        public ServiceRequest ServiceRequest { get; }
        
        public EmploymentCheckRequestAudit(
            Guid id,
            Guid apprenticeshipIncentiveId,
            EmploymentCheckType checkType,
            ServiceRequest serviceRequest)
        {
            Id = id;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            CheckType = checkType;
            ServiceRequest = serviceRequest;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return ApprenticeshipIncentiveId;
            yield return CheckType;
            yield return ServiceRequest;
        }
    }
}
