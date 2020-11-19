using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity;

namespace SFA.DAS.EmployerIncentives.Commands.AddEmployerVendorIdForLegalEntity
{
    public class AddEmployerVendorIdForLegalEntityCommandValidator : IValidator<AddEmployerVendorIdForLegalEntityCommand>
    {
        public Task<ValidationResult> Validate(AddEmployerVendorIdForLegalEntityCommand item)
        {
            var result = new ValidationResult();

            if (item.HashedLegalEntityId == default)
            {
                result.AddError("HashedLegalEntityId", "Is not set");
            }

            if (item.EmployerVendorId == default)
            {
                result.AddError("EmployerVendorId", "Is not set");
            }
            else if (item.EmployerVendorId.All(c => c == '0'))
            {
                result.AddError("EmployerVendorId", "Is not set to a valid Id");
            }

            return Task.FromResult(result);
        }      
    }
}
