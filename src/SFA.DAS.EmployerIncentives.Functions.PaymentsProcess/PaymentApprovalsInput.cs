namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class PaymentApprovalsInput
    {
        public PaymentApprovalsInput(
            CollectionPeriod collectionPeriod,
            string paymentOrchestrationId)
        {
            CollectionPeriod = collectionPeriod;
            PaymentOrchestrationId = paymentOrchestrationId;
        }

        public CollectionPeriod CollectionPeriod { get; }

        public string PaymentOrchestrationId { get; }
    }
}
