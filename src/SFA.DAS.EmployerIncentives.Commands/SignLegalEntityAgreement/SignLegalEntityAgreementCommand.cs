using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement
{    
    public class SignLegalEntityAgreementCommand : ICommand
    {
        public long AccountLegalEntityId { get; }
        public int AgreementVersion { get; }

        public SignLegalEntityAgreementCommand(
            long accountLegalEntityId,
            int agreementVersion)
        {
            AccountLegalEntityId = accountLegalEntityId;
            AgreementVersion = agreementVersion;
        }
    }
}
