using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Abstractions.Events
{
    public class AddEmployerVendorIdCommand : ICommand
    {
        public string HashedLegalEntityId { get; set; }
    }
}