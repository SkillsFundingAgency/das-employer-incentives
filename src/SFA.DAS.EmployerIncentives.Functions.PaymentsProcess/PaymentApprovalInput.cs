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
            PaymentOrchestrationId = paymentOrchestrationId;
        }

        public CollectionPeriod CollectionPeriod { get; }
        public string EmailAddress { get; }
        public string PaymentOrchestrationId { get; }
    }
}
