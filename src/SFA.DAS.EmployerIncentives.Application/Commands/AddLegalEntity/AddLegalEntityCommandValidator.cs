using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity
{
    public class AddLegalEntityCommandValidator : IValidator<AddLegalEntityCommand>
    {
        public Task<ValidationResult> Validate(AddLegalEntityCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
