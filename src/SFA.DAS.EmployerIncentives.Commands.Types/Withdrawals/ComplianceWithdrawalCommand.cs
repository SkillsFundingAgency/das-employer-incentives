using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals
{
    public class ComplianceWithdrawalCommand : WithdrawalCommand, IPeriodEndIncompatible
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

        public TimeSpan CommandDelay => TimeSpan.FromMinutes(15);
        public bool CancelCommand => false;
    }
}
