
using System;

namespace SFA.DAS.EmployerIncentives.ManualPaymentsTemplate
{
    public class PaymentRecord
    {
        public string DocumentType { get; set; }
        public string AccountNumber { get; set; }
        public string FundingTypeCode { get; set; }
        public decimal Values { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public long GLAccountCode { get; set; }
        public string ExtRef4 { get; set; }
        public string CostCentreCodeDimension2 { get; set; }
        public string ExtRef3Submitter { get; set; }
        public string RemittanceDescription { get; set; }
    }
}
