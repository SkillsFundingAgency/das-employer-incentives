namespace SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity
{
    public class AddLegalEntityCommand : ICommand
    {
        public long LegalentityId {get; private set;}

        public AddLegalEntityCommand(long legalentityId)
        {            
            LegalentityId = legalentityId;
        }
    }
}
