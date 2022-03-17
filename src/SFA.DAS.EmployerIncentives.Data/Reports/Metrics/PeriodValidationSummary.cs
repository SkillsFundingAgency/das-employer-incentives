namespace SFA.DAS.EmployerIncentives.Data.Reports.Metrics
{
    public class PeriodValidationSummary
    {
        public ValidationSummaryRecord ValidRecords { get; set; }
        public ValidationSummaryRecord InvalidRecords { get; set; }

        public class ValidationSummaryRecord
        {
            public int Count { get; set; }
            public double PeriodAmount { get; set; }
            public double YtdAmount { get; set; }
        }
    }
}
