using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments
{
    public class PausePaymentsCommandHandler : ICommandHandler<PausePaymentsCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
 
        public PausePaymentsCommandHandler(IApprenticeshipIncentiveDomainRepository incentiveDomainRepository)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
        }

        public async Task Handle(PausePaymentsCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.FindByUlnWithinAccountLegalEntity(command.ULN, command.AccountLegalEntityId);

            if(incentive == null)
            {
                throw new KeyNotFoundException($"Unable to handle pause payments command as no incentive was found for ULN {command.ULN} and Account Legal Entity {command.AccountLegalEntityId}");
            }

            var serviceRequest = new ServiceRequest(command.ServiceRequestId, command.DecisionReferenceNumber, command.DateServiceRequestTaskCreated);

            switch (command.Action)
            {
                case PausePaymentsAction.Pause:
                    incentive.PauseSubsequentPayments(serviceRequest);
                    break;

                case PausePaymentsAction.Resume:
                    incentive.ResumePayments(serviceRequest);
                    break;
                default:
                    throw new PausePaymentsException("Pause and Resume are the only supported actions");
            }

            await _incentiveDomainRepository.Save(incentive);
        }
    }
}
