using System;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals.EmployerWithdrawal
{
    public class EmployerWithdrawalCommand : WithdrawalCommand
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
    }
}
