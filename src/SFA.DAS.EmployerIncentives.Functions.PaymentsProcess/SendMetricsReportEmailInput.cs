namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class SendMetricsReportEmailInput
    {
        public CollectionPeriod CollectionPeriod { get; set; }
        public string EmailAddress { get; set; }
        public string ApprovalLink { get; set; }
    }
}
