using System;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string VendorId { get; set; }
        public DateTime DueDate { get; set; }
        public SubnominalCode SubnominalCode { get; set; }
        public decimal Amount { get; set; }
        public EarningType EarningType { get; set; }
        public long ULN { get; set; }
        public string HashedLegalEntityId { get; set; }
    }
}
