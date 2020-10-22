using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidatePendingPayment
{
    public class ValidatePendingPaymentCommandHandler : ICommandHandler<ValidatePendingPaymentCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly ISpecificationsFactory<PendingPayment> _specificationsFactory;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public ValidatePendingPaymentCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository,
            ISpecificationsFactory<PendingPayment> specificationsFactory,
            ICollectionCalendarService collectionCalendarService)
        {
            _domainRepository = domainRepository;
            _specificationsFactory = specificationsFactory;
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(ValidatePendingPaymentCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);
            
            var calendar = await _collectionCalendarService.Get();
            var collectionPeriod = calendar.GetPeriod(command.CollectionYear, command.CollectionMonth);

            await incentive.ValidatePendingPayment(command.PendingPaymentId, _specificationsFactory.Rules, collectionPeriod);

            await _domainRepository.Save(incentive);
        }
    }
}
