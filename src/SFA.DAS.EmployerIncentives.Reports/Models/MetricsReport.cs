using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Reports.Models
{
    public class MetricsReport
    {
        public string Name { get; private set; }

        public CollectionPeriod CollectionPeriod { get; set; }
        public IEnumerable<PaymentsMade> PaymentsMade { get; set; }
        public IEnumerable<Earning> Earnings { get; set; }
        public PeriodValidationSummary ValidationSummary { get; set; }
        public Clawbacks Clawbacks { get; set; }
        public IEnumerable<YtdValidation> YtdValidations { get; set; }

        public MetricsReport(string name = "Metrics")
        {
            Name = name;
            PaymentsMade = new List<PaymentsMade>();
            Earnings = new List<Earning>();
            YtdValidations = new List<YtdValidation>();
        }
        public void SetName(string name)
        {
            Name = name;
        }
    }
}
