using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class IncentiveApplicationAudit : ValueObject
    {
        public Guid Id { get; set; }

        public Guid IncentiveApplicationApprenticeshipId { get; set; }

        public IncentiveApplicationStatus Process { get; set; }

        public ServiceRequest ServiceRequest { get; }
        
        public IncentiveApplicationAudit(
            Guid id,
            Guid incentiveApplicationApprenticeshipId,
            IncentiveApplicationStatus process,
            ServiceRequest serviceRequest)
        {
            Id = id;
            IncentiveApplicationApprenticeshipId = incentiveApplicationApprenticeshipId;
            Process = process;
            ServiceRequest = serviceRequest;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return IncentiveApplicationApprenticeshipId;
            yield return Process;
            yield return ServiceRequest;
        }
    }
}
