using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ValidationOverrides;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ValidationOverride
{
    public class ValidationOverrideCommandHandler : ICommandHandler<ValidationOverrideCommand>
    {
        public ValidationOverrideCommandHandler()
        {
        }

        public async Task Handle(ValidationOverrideCommand command, CancellationToken cancellationToken = default)
        {
            // TODO:
            // 1. Look up the apprenticeship incentive the validation override is to be applied to
            // 2. Throw exception if the apprenticeship incentive does not exist
            // 3. Pass the validation override step and service request value types to the incentive to apply the override
            // 4. The method on the incentive should remove existing ValidationOverrides (and raise a Deleted event)
            //   and add the new ValidationOverrides to the incentive (and raise a Created event).
            // 5. Persist the changes 

            await Task.CompletedTask;
        }
    }
}
