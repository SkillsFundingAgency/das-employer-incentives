using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive
{
    public class CreateIncentiveCommandHandler : ICommandHandler<CreateIncentiveCommand>
    {
        private readonly IApprenticeshipIncentiveFactory _apprenticeshipIncentiveFactory;
        private readonly IApprenticeshipIncentiveDomainRepository _apprenticeshipIncentiveDomainRepository;

        public CreateIncentiveCommandHandler(
            IApprenticeshipIncentiveFactory apprenticeshipIncentiveFactory,
            IApprenticeshipIncentiveDomainRepository apprenticeshipIncentiveDomainRepository)
        {
            _apprenticeshipIncentiveFactory = apprenticeshipIncentiveFactory;
            _apprenticeshipIncentiveDomainRepository = apprenticeshipIncentiveDomainRepository;
        }

        public async Task Handle(CreateIncentiveCommand command, CancellationToken cancellationToken = default)
        {
            foreach (var incentive in command.Apprenticeships.Select(apprenticeship => _apprenticeshipIncentiveFactory.CreateNew(
                Guid.NewGuid(),
                apprenticeship.IncentiveApplicationApprenticeshipId,
                new Account(command.AccountId, command.AccountLegalEntityId),
                new Apprenticeship(
                    apprenticeship.ApprenticeshipId,
                    apprenticeship.FirstName,
                    apprenticeship.LastName,
                    apprenticeship.DateOfBirth,
                    apprenticeship.Uln,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval
                ),
                apprenticeship.PlannedStartDate)))
            {
                await _apprenticeshipIncentiveDomainRepository.Save(incentive);
            }
        }
    }
}
