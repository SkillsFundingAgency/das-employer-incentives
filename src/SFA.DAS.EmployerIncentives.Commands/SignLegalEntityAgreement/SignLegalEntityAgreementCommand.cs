using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement
{    
    public class SignLegalEntityAgreementCommand : ICommand
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public int AgreementVersion { get; }

        public SignLegalEntityAgreementCommand(
            long accountId,
            long accountLegalEntityId,
            int agreementVersion)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
            AgreementVersion = agreementVersion;
        }
    }
}
