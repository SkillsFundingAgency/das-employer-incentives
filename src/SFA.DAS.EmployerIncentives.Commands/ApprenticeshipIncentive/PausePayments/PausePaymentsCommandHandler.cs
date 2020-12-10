﻿using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

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
                throw new PausePaymentsException($"Unable to handle pause payments command as no incentive was found for ULN {command.ULN} and Account Legal Entity {command.AccountLegalEntityId}");
            }

            var serviceRequest = new ServiceRequest(command.ServiceRequestId, command.DecisionReferenceNumber, command.DateServiceRequestTaskCreated);
            incentive.PauseSubsequentPayments(serviceRequest);

            await _incentiveDomainRepository.Save(incentive);
        }
    }
}