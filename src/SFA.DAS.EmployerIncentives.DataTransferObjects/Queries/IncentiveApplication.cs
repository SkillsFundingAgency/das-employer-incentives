using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.DataTransferObjects.Queries
{
    public class IncentiveApplication
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long LegalEntityId { get; set; }
        public string SubmittedByEmail { get; set; }
        public string SubmittedByName { get; set; }
        public bool BankDetailsRequired { get; set; }
        public bool NewAgreementRequired { get; set; }
        public LegalEntity LegalEntity { get; set; }
        public IEnumerable<IncentiveApplicationApprenticeship> Apprenticeships { get; set; }
    }
}
