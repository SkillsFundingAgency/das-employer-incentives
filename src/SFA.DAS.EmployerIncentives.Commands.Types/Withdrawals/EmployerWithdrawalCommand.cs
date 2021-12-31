using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals
{
    public class EmployerWithdrawalCommand : WithdrawalCommand, IPeriodEndIncompatible
    {
        public EmployerWithdrawalCommand(
            long accountLegalEntityId,
            long uln,
            string serviceRequestTaskId,
            string decisionReference,
            DateTime serviceRequestCreated,
            long accountId,
            string emailAddress)
            : base(
                  accountLegalEntityId,
                  uln,
                  serviceRequestTaskId,
                  decisionReference,
                  serviceRequestCreated)
        {
            AccountId = accountId;
            EmailAddress = emailAddress;
        }

        public TimeSpan CommandDelay => TimeSpan.FromMinutes(15);
        public bool CancelCommand => false;
        public long AccountId { get; }
        public string EmailAddress { get; }
    }
}
