using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals
{
    public class ReinstateWithdrawalCommand : DomainCommand, IPeriodEndIncompatible
    {  
        public long AccountLegalEntityId { get; }
        public long ULN { get; }

        public ReinstateWithdrawalCommand(            
            long accountLegalEntityId,
            long uln)
        {
            AccountLegalEntityId = accountLegalEntityId;
            ULN = uln;
        }

        public TimeSpan CommandDelay => TimeSpan.FromMinutes(15);
        public bool CancelCommand => false;
    }
}
