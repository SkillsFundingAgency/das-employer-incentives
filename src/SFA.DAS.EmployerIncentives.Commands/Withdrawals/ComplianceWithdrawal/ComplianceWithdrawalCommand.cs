using System;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals.ComplianceWithdrawal
{
    public class ComplianceWithdrawalCommand : WithdrawalCommand
    {  
        public ComplianceWithdrawalCommand(            
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
