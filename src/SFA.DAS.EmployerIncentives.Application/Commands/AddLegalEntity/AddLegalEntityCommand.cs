namespace SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity
{
    public class AddLegalEntityCommand : ICommand
    {
        public long AccountId { get; private set; }
        public long LegalEntityId { get; private set; }
        public string Name { get; set; }
        public long AccountLegalEntityId { get; set; }

        public AddLegalEntityCommand(
            long accountId,
            long legalEntityId,
            string name,
            long accountLegalEntityId)
        {
            AccountId = accountId;
            LegalEntityId = legalEntityId;
            Name = name;
            AccountLegalEntityId = accountLegalEntityId;
        }
    }
}
