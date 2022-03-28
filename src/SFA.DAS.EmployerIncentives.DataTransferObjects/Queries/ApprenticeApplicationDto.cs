using System;

namespace SFA.DAS.EmployerIncentives.DataTransferObjects.Queries
{
    public class ApprenticeApplicationDto
    {
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long ULN { get; set; }
        public DateTime ApplicationDate { get; set; }
        public decimal TotalIncentiveAmount { get; set; }
        public string CourseName { get; set; }
        public string SubmittedByEmail { get; set; }
        public PaymentStatusDto FirstPaymentStatus { get; set; }
        public PaymentStatusDto SecondPaymentStatus { get; set; }
        public ClawbackStatusDto FirstClawbackStatus { get; set; }
        public ClawbackStatusDto SecondClawbackStatus { get; set; }
    }

}
