using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class PaymentProfile
    {
        public IncentiveType IncentiveType { get; set; }
        public int DaysAfterApprenticeshipStart { get; set; }
        public decimal AmountPayable { get; set; }
    }
}