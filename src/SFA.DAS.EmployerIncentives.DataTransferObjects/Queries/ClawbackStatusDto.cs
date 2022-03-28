using System;

namespace SFA.DAS.EmployerIncentives.DataTransferObjects.Queries
{
    public class ClawbackStatusDto
    {
        public decimal ClawbackAmount { get; set; }
        public DateTime? ClawbackDate { get; set; }
        public DateTime? OriginalPaymentDate { get; set; }
    }
}
