namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class PaymentApprovalInput
    {
        public PaymentApprovalInput(
            CollectionPeriod collectionPeriod,
            string emailAddress,
            string paymentOrchestrationId)
        {
            CollectionPeriod = collectionPeriod;
            EmailAddress = emailAddress;
            EmailSent = false;
            RemindersSent = 0;
            PaymentOrchestrationId = paymentOrchestrationId;
        }

        public CollectionPeriod CollectionPeriod { get; }
        public string EmailAddress { get; }

        public bool EmailSent { get; set;  }
        public int RemindersSent { get; set; }
        public string PaymentOrchestrationId { get; }
        public string PaymentApprovalOrchestrationId { get; set;  }
    }
}
