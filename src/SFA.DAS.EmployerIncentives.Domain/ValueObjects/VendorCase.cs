using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class VendorCase : ValueObject
    {
        public VendorCase(string caseId, string status, DateTime? updated)
        {
            CaseId = caseId;
            Status = status;
            Updated = updated;
        }

        public string CaseId { get; }
        public string Status { get; }
        public DateTime? Updated { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return CaseId;
            yield return Status;
            yield return Updated;            
        }
    }
}