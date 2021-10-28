using System;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class EarningsCalculationValidation
    {
        public long AccountId { get; private set; }
        public long ApprenticeshipId { get; private set; }
        public Guid IncentiveApplicationApprenticeshipId { get; private set; }

        public EarningsCalculationValidation(long accountId, long apprenticeshipId, Guid incentiveApplicationApprenticeshipId)
        {
            AccountId = accountId;
            ApprenticeshipId = apprenticeshipId;
            IncentiveApplicationApprenticeshipId = incentiveApplicationApprenticeshipId;
        }
    }
}
