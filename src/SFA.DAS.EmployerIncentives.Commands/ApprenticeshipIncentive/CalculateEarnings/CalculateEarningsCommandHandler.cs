using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateEarnings
{
    public class CalculateEarningsCommandHandler : ICommandHandler<CalculateEarningsCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly IIncentivePaymentProfilesService _incentivePaymentProfilesService;
        private readonly ICollectionCalendarService _collectionCalendarService;
        private readonly IMessageSession _messageSession;

        public CalculateEarningsCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository, 
            IIncentivePaymentProfilesService incentivePaymentProfilesService,
            ICollectionCalendarService collectionCalendarService,
            IMessageSession messageSession)
        {
            _domainRepository = domainRepository;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
            _collectionCalendarService = collectionCalendarService;
            _messageSession = messageSession;
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
            var sendOptions = new SendOptions();
            sendOptions.DelayDeliveryWith(TimeSpan.FromHours(1));
            await _messageSession.Send(new CalculateEarningsCommand(command.ApprenticeshipIncentiveId), sendOptions);
        }

        private async Task<bool> ActivePeriodInProgress()
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            var activePeriod = collectionCalendar.GetActivePeriod();
            return activePeriod.PeriodEndInProgress;
        }
    }
}
