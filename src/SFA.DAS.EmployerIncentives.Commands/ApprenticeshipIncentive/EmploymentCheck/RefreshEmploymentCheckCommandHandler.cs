using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck
{
    public class RefreshEmploymentCheckCommandHandler : ICommandHandler<RefreshEmploymentCheckCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly IDateTimeService _dateTimeService;

        public RefreshEmploymentCheckCommandHandler(
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            IDateTimeService dateTimeService)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _dateTimeService = dateTimeService;
        }
        public async Task Handle(RefreshEmploymentCheckCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var incentive = await _incentiveDomainRepository.FindByUlnWithinAccountLegalEntity(command.ULN, command.AccountLegalEntityId);

            if (incentive == null)
            {
                throw new ArgumentException($"Apprenticeship incentive with account legal entity of {command.AccountLegalEntityId} and ULN {command.ULN} not found");
            }

            incentive.RefreshEmploymentChecks(
                _dateTimeService,
                new ServiceRequest(
                    command.ServiceRequestTaskId, 
                    command.DecisionReference, 
                    command.ServiceRequestCreated),
                command.CheckType);

            await _incentiveDomainRepository.Save(incentive);
        }
    }

}
