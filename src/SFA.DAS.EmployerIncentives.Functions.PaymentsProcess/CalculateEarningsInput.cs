using System;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CalculateEarningsInput
    {
        public CalculateEarningsInput(Guid apprenticeshipIncentiveId, long uln)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            Uln = uln;
        }

        public Guid ApprenticeshipIncentiveId { get; }
        public long Uln { get; }
    }
}
