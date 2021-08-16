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
        private readonly IDateTimeService _dateTimeService;

        public CalculateEarningsCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository, 
            IIncentivePaymentProfilesService incentivePaymentProfilesService,
            ICollectionCalendarService collectionCalendarService,
            IDateTimeService dateTimeService)
        {
            _domainRepository = domainRepository;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
            _collectionCalendarService = collectionCalendarService;
            _dateTimeService = dateTimeService;
        }

        public async Task Handle(CalculateEarningsCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);

            await incentive.CalculateEarnings(_incentivePaymentProfilesService, _collectionCalendarService, _dateTimeService);

            await _domainRepository.Save(incentive);
        }
    }
}
