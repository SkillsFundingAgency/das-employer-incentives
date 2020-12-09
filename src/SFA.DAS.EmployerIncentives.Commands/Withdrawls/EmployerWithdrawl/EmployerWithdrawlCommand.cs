using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawls.EmployerWithdrawl
{
    public class EmployerWithdrawlCommand : ICommand
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

        public EmployerWithdrawlCommand(            
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
