using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive
{
    public class CreateApprenticeshipIncentiveCommandHandler : ICommandHandler<CreateApprenticeshipIncentiveCommand>
    {
        private readonly IApprenticeshipIncentiveFactory _apprenticeshipIncentiveFactory;
        private readonly IApprenticeshipIncentiveDomainRepository _apprenticeshipIncentiveDomainRepository;

        public CreateApprenticeshipIncentiveCommandHandler(
            IApprenticeshipIncentiveFactory apprenticeshipIncentiveFactory,
            IApprenticeshipIncentiveDomainRepository apprenticeshipIncentiveDomainRepository)
        {
            _apprenticeshipIncentiveFactory = apprenticeshipIncentiveFactory;
            _apprenticeshipIncentiveDomainRepository = apprenticeshipIncentiveDomainRepository;
        }

        public async Task Handle(CreateApprenticeshipIncentiveCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = _apprenticeshipIncentiveFactory.CreateNew(
                Guid.NewGuid(),
                command.IncentiveApplicationApprenticeshipId,
                new Account(command.AccountId),
                new Apprenticeship(
                    command.ApprenticeshipId,
                    command.FirstName,
                    command.LastName,
                    command.DateOfBirth,
                    command.Uln,
                    command.ApprenticeshipEmployerTypeOnApproval
                ),
                command.PlannedStartDate);

            await _apprenticeshipIncentiveDomainRepository.Save(incentive);
        }
    }
}
