using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive
{
    public class CreateIncentiveCommandHandler : ICommandHandler<CreateIncentiveCommand>
    {
        private readonly IApprenticeshipIncentiveFactory _apprenticeshipIncentiveFactory;
        private readonly IApprenticeshipIncentiveDomainRepository _apprenticeshipIncentiveDomainRepository;
        private readonly ICommandPublisher _commandPublisher;

        public CreateIncentiveCommandHandler(
            IApprenticeshipIncentiveFactory apprenticeshipIncentiveFactory,
            IApprenticeshipIncentiveDomainRepository apprenticeshipIncentiveDomainRepository,
            ICommandPublisher commandPublisher)
        {
            _apprenticeshipIncentiveFactory = apprenticeshipIncentiveFactory;
            _apprenticeshipIncentiveDomainRepository = apprenticeshipIncentiveDomainRepository;
            _commandPublisher = commandPublisher;
        }

        public async Task Handle(CreateIncentiveCommand command, CancellationToken cancellationToken = default)
        {
            var existing = await _apprenticeshipIncentiveDomainRepository.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId);
            if (existing != null)
            {
                if (existing.PendingPayments.Count > 0)
                {
                    var completeEarningCalculationCommand = new CompleteEarningsCalculationCommand(existing.Account.Id,
                        command.IncentiveApplicationApprenticeshipId, existing.Apprenticeship.Id, existing.Id);
                    await _commandPublisher.Publish(completeEarningCalculationCommand);
                }

                return;
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
                    command.EmploymentStartDate
                ),
                command.PlannedStartDate,
                command.SubmittedDate,
                command.SubmittedByEmail,
                AgreementVersion.Create(command.Phase, command.PlannedStartDate),
                new IncentivePhase(command.Phase)
                );

            if (command.UKPRN.HasValue)
            {
                incentive.Apprenticeship.SetProvider(new Provider(command.UKPRN.Value));
            }

            await _apprenticeshipIncentiveDomainRepository.Save(incentive);
        }
    }
}
