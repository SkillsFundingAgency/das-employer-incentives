using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals
{
    public class EmployerWithdrawalCommand : WithdrawalCommand , IPeriodEndIncompatible
    {
        public EmployerWithdrawalCommand(
            long accountLegalEntityId,
            long uln,
            string serviceRequestTaskId,
            string decisionReference,
            DateTime serviceRequestCreated)
            : base(
                  accountLegalEntityId,
                  uln,
                  serviceRequestTaskId,
                  decisionReference,
                  serviceRequestCreated)
        {
        }

        public TimeSpan CommandDelay => TimeSpan.FromMinutes(15);
    }
}
