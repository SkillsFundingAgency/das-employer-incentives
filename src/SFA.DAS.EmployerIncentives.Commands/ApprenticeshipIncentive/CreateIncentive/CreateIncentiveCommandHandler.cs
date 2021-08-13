using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Enums;

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
            var existing = await _apprenticeshipIncentiveDomainRepository.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId);
            if (existing != null)
            {
                return;
            }

            Provider provider = null;
            if (command.UKPRN.HasValue)
            {
                provider = new Provider(command.UKPRN.Value);
            }

            var incentive = _apprenticeshipIncentiveFactory.CreateNew(
                Guid.NewGuid(),
                command.IncentiveApplicationApprenticeshipId,
                new Account(command.AccountId, command.AccountLegalEntityId),
                new Apprenticeship(
                    command.ApprenticeshipId,
                    command.FirstName,
                    command.LastName,
                    command.DateOfBirth,
                    command.Uln,
                    command.ApprenticeshipEmployerTypeOnApproval,
                    command.CourseName,
                    command.EmploymentStartDate,
                    provider
                ),
                command.PlannedStartDate,
                command.SubmittedDate,
                command.SubmittedByEmail,
                AgreementVersion.Create(command.Phase, command.PlannedStartDate),
                new IncentivePhase(command.Phase)
                );

            await _apprenticeshipIncentiveDomainRepository.Save(incentive);
        }
    }
}
