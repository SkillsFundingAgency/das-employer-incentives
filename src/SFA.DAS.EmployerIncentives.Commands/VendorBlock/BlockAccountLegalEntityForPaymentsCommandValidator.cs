using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.VendorBlock
{
    public class BlockAccountLegalEntityForPaymentsCommandValidator : IValidator<BlockAccountLegalEntityForPaymentsCommand>
    {
        public Task<ValidationResult> Validate(BlockAccountLegalEntityForPaymentsCommand item)
        {
            var result = new ValidationResult();

            if (item.VendorId == default)
            {
                result.AddError("VendorId", "Is not set");
            }

            if (item.VendorBlockEndDate < DateTime.Now)
            {
                result.AddError("VendorBlockEndDate", "Is invalid");
            }
            else if (item.VendorBlockEndDate == default)
            {
                result.AddError("VendorBlockEndDate", "Is not set");
            }

            if (item.ServiceRequestTaskId == default)
            {
                result.AddError("ServiceRequestTaskId", "Is not set");
            }

            if (item.ServiceRequestDecisionReference == default)
            {
                result.AddError("ServiceRequestDecisionReference", "Is not set");
            }
            
            return Task.FromResult(result);
        }
    }
}
