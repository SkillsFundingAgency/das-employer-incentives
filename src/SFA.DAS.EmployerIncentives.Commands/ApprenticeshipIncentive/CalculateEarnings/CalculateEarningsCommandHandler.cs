using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateEarnings
{
    public class CalculateEarningsCommandHandler : ICommandHandler<CalculateEarningsCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly IIncentivePaymentProfilesService _incentivePaymentProfilesService;
        private readonly ICollectionCalendarService _collectionCalendarService;
        private readonly ILogger<CalculateEarningsCommandHandler> _logger;

        public CalculateEarningsCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository, 
            IIncentivePaymentProfilesService incentivePaymentProfilesService,
            ICollectionCalendarService collectionCalendarService,
            ILogger<CalculateEarningsCommandHandler> logger)
        {
            _domainRepository = domainRepository;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
            _collectionCalendarService = collectionCalendarService;
            _logger = logger;
        }

        public async Task Handle(CalculateEarningsCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);

            var paymentProfiles = await _incentivePaymentProfilesService.Get();

            // for testing
            foreach(var incentiveType in paymentProfiles)
            {
                foreach(var profile in incentiveType.PaymentProfiles)
                {
                    _logger.LogInformation($"Incentive {incentiveType.IncentiveType} Profile {profile.DaysAfterApprenticeshipStart} days {profile.AmountPayable} paid");
                }
            }
            // for testing

            var collectionCalendar = await _collectionCalendarService.Get();
            incentive.CalculateEarnings(paymentProfiles, collectionCalendar);

            await _domainRepository.Save(incentive);
        }
    }
}
