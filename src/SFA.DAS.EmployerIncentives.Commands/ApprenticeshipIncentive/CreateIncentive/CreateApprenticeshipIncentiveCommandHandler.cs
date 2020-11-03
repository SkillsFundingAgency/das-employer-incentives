using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive
{
    public class CreateApprenticeshipIncentiveCommandHandler : ICommandHandler<CreateApprenticeshipIncentiveCommand>
    {
        private readonly IApprenticeshipIncentiveFactory _apprenticeshipIncentiveFactory;
        private readonly IApprenticeshipIncentiveDomainRepository _apprenticeshipIncentiveDomainRepository;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public CreateApprenticeshipIncentiveCommandHandler(
            IApprenticeshipIncentiveFactory apprenticeshipIncentiveFactory,
            IApprenticeshipIncentiveDomainRepository apprenticeshipIncentiveDomainRepository,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _apprenticeshipIncentiveFactory = apprenticeshipIncentiveFactory;
            _apprenticeshipIncentiveDomainRepository = apprenticeshipIncentiveDomainRepository;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task Handle(CreateApprenticeshipIncentiveCommand command, CancellationToken cancellationToken = default)
        {
            var existing = await _apprenticeshipIncentiveDomainRepository.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId);
            if (existing != null)
            {
                if (existing.PendingPayments.Count() == 0)
                {
                    var calculatePaymentsEvent = new Created
                    {
                        AccountId = command.AccountId,
                        ApprenticeshipId = command.ApprenticeshipId,
                        ApprenticeshipIncentiveId = existing.Id
                    };
                    await _domainEventDispatcher.Send(calculatePaymentsEvent);
                }
                return;
            }

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
