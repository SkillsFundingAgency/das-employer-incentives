using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class VendorBlockRequestAudit : ValueObject
    {
        public Guid Id { get; }
        public string VrfVendorId { get; set; }
        public DateTime VendorBlockEndDate { get; set; }
        public ServiceRequest ServiceRequest { get; }
        
        public VendorBlockRequestAudit(
            Guid id,
            string vrfVendorId,
            DateTime vendorBlockEndDate,
            ServiceRequest serviceRequest)
        {
            Id = id;
            VrfVendorId = vrfVendorId;
            VendorBlockEndDate = vendorBlockEndDate;
            ServiceRequest = serviceRequest;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return VrfVendorId;
            yield return VendorBlockEndDate;
            yield return ServiceRequest;
        }
    }
}
