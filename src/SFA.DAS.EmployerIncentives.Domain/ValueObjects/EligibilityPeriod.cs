using System;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class EligibilityPeriod
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public int MinimumAgreementVersion { get; }

        public EligibilityPeriod(DateTime startDate, DateTime endDate, int minimumAgreementVersion)
        {
            StartDate = startDate;
            EndDate = endDate;
            MinimumAgreementVersion = minimumAgreementVersion;
        }
    }
}
