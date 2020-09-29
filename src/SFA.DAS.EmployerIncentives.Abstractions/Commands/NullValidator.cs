using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Abstractions.Commands
{
    public class NullValidator : IValidator<DomainCommand>
    {
        public Task<ValidationResult> Validate(DomainCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
