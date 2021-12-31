using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class EmploymentCheckRequestAudit : ValueObject
    {
        public Guid Id { get; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public ServiceRequest ServiceRequest { get; }
        
        public EmploymentCheckRequestAudit(
            Guid id,
            Guid apprenticeshipIncentiveId,
            ServiceRequest serviceRequest)
        {
            Id = id;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            ServiceRequest = serviceRequest;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return ApprenticeshipIncentiveId;
            yield return ServiceRequest;
        }
    }
}
