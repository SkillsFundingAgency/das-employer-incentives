using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidationOverrides;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidationOverride
{
    public class ValidationOverrideCommandHandler : ICommandHandler<ValidationOverrideCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;

        public ValidationOverrideCommandHandler(IApprenticeshipIncentiveDomainRepository incentiveDomainRepository)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
        }

        public async Task Handle(ValidationOverrideCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.FindByUlnWithinAccountLegalEntity(command.ULN, command.AccountLegalEntityId);

            if (incentive == null)
            {
                throw new KeyNotFoundException($"Unable to handle validation override command as no incentive was found for ULN {command.ULN} and Account Legal Entity {command.AccountLegalEntityId}");
            }

            var serviceRequest = new ServiceRequest(command.ServiceRequestTaskId, command.DecisionReference, command.ServiceRequestCreated);

            command.ValidationSteps.ToList().ForEach(s => incentive.AddValidationOverride(s, serviceRequest));

            await _incentiveDomainRepository.Save(incentive);
        }
    }
}

