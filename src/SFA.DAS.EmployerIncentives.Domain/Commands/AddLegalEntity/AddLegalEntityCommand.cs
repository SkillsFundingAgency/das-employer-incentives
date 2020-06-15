namespace SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity
{
    public class AddLegalEntityCommand : ICommand
    {
        public int LegalentityId {get; private set;}

        public AddLegalEntityCommand(int legalentityId)
        {
            LegalentityId = legalentityId;
        }
    }
}
