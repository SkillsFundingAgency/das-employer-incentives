using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class IncompleteEarningsCalculationCheckCommandHandler : ICommandHandler<IncompleteEarningsCalculationCheckCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _applicationDomainRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        public IncompleteEarningsCalculationCheckCommandHandler(IIncentiveApplicationDomainRepository applicationDomainRepository,
            ICommandDispatcher commandDispatcher)
        {
            _applicationDomainRepository = applicationDomainRepository;
            _commandDispatcher = commandDispatcher;
        }

        public async Task Handle(IncompleteEarningsCalculationCheckCommand command,  CancellationToken cancellationToken = default(CancellationToken))
        {
            var applications = await _applicationDomainRepository.FindIncentiveApplicationsWithoutEarningsCalculations();

            var earningsCalculationValidations = new List<EarningsCalculationValidation>();
            foreach (var application in applications)
            {
                foreach(var apprenticeship in application.Apprenticeships)
                {
                    earningsCalculationValidations.Add(new EarningsCalculationValidation(application.AccountId, apprenticeship.ApprenticeshipId, apprenticeship.Id));
                }
            }

            await _commandDispatcher.Send(new ValidateIncompleteEarningsCalculationCommand(earningsCalculationValidations));
        }
    }

}
