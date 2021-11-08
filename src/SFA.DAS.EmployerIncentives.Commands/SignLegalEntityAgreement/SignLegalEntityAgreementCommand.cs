using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement
{    
    public class SignLegalEntityAgreementCommand : ICommand
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public int AgreementVersion { get; }
        public string LegalEntityName { get; }
        public long LegalEntityId { get; }

        public SignLegalEntityAgreementCommand(
            long accountId,
            long accountLegalEntityId,
            int agreementVersion,
            string legalEntityName,
            long legalEntityId)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
            AgreementVersion = agreementVersion;
            LegalEntityName = legalEntityName;
            LegalEntityId = legalEntityId;
        }
    }
}
