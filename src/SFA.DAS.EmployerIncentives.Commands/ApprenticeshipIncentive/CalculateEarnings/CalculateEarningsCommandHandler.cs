using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateEarnings
{
    public class CalculateEarningsCommandHandler : ICommandHandler<CalculateEarningsCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly IIncentivePaymentProfilesService _incentivePaymentProfilesService;
        private readonly ICollectionCalendarService _collectionCalendarService;
        private readonly IScheduledCommandPublisher _commandPublisher;

        public CalculateEarningsCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository, 
            IIncentivePaymentProfilesService incentivePaymentProfilesService,
            ICollectionCalendarService collectionCalendarService,
            IScheduledCommandPublisher commandPublisher)
        {
            _domainRepository = domainRepository;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
            _collectionCalendarService = collectionCalendarService;
            _commandPublisher = commandPublisher;
        }

        public async Task Handle(CalculateEarningsCommand command, CancellationToken cancellationToken = default)
        {
            if(await ActivePeriodInProgress())
            {
                await ScheduleCalculateEarnings(command);
                return;
            }

            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);

            await incentive.CalculateEarnings(_incentivePaymentProfilesService, _collectionCalendarService);

            await _domainRepository.Save(incentive);
        }

        private async Task ScheduleCalculateEarnings(CalculateEarningsCommand command)
        {
            await _commandPublisher.Send(new CalculateEarningsCommand(command.ApprenticeshipIncentiveId), TimeSpan.FromHours(1));
        }

        private async Task<bool> ActivePeriodInProgress()
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            var activePeriod = collectionCalendar.GetActivePeriod();
            return activePeriod.PeriodEndInProgress;
        }
    }
}
