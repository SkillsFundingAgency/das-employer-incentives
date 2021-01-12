using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals
{
    public abstract class WithdrawalCommand : ICommand
    {   
        public long AccountLegalEntityId { get; }
        public long ULN { get; }
        public string ServiceRequestTaskId { get; }
        public string DecisionReference { get; }
        public DateTime ServiceRequestCreated { get; }

        public override string ToString()
        {
            return $"AccountLegalEntityId:{AccountLegalEntityId}, ULN:{ULN}, ServiceRequestTaskId:{ServiceRequestTaskId}";
        }

        protected WithdrawalCommand(            
            long accountLegalEntityId,
            long uln,
            string serviceRequestTaskId,
            string decisionReference,
            DateTime serviceRequestCreated)
        {
            AccountLegalEntityId = accountLegalEntityId;
            ULN = uln;
            ServiceRequestTaskId = serviceRequestTaskId;
            DecisionReference = decisionReference;
            ServiceRequestCreated = serviceRequestCreated;
        }
    }
}
