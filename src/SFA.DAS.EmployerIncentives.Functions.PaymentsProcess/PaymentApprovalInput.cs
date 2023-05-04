namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class PaymentApprovalInput
    {
        public PaymentApprovalInput(
            CollectionPeriod collectionPeriod,
            string emailAddress,
            string paymentOrchestrationId,
            string correlationId)
        {
            CollectionPeriod = collectionPeriod;
            EmailAddress = emailAddress;
            PaymentOrchestrationId = paymentOrchestrationId;
            CorrelationId = correlationId;
        }

        public CollectionPeriod CollectionPeriod { get; }
        public string EmailAddress { get; }
        public string PaymentOrchestrationId { get; }
        public string PaymentApprovalOrchestrationId { get; set; }
        public string CorrelationId { get; }
        public bool IsResend { get; set; }
    }
}
