using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidatePendingPayment
{
    public class ValidatePendingPaymentCommandHandler : ICommandHandler<ValidatePendingPaymentCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly IAccountDomainRepository _accountDomainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly IIncentivePaymentProfilesService _incentivePaymentProfilesService;

        public ValidatePendingPaymentCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository,
            IAccountDomainRepository accountDomainRepository,
            ICollectionCalendarService collectionCalendarService,
            ILearnerDomainRepository learnerDomainRepository,
            IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            _domainRepository = domainRepository;
            _accountDomainRepository = accountDomainRepository;
            _collectionCalendarService = collectionCalendarService;
            _learnerDomainRepository = learnerDomainRepository;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
        }

        public async Task Handle(ValidatePendingPaymentCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);
            var account = await _accountDomainRepository.Find(incentive.Account.Id);
            var learner = await _learnerDomainRepository.GetOrCreate(incentive);

            var calendar = await _collectionCalendarService.Get();
            var collectionCalendarPeriod = calendar.GetPeriod(new Domain.ValueObjects.CollectionPeriod(command.CollectionPeriod, command.CollectionYear));

            incentive.ValidatePendingPaymentBankDetails(command.PendingPaymentId, account, collectionCalendarPeriod.CollectionPeriod);
            incentive.ValidateLearningData(command.PendingPaymentId, learner, collectionCalendarPeriod.CollectionPeriod, await _incentivePaymentProfilesService.Get());
            incentive.ValidatePaymentsNotPaused(command.PendingPaymentId, collectionCalendarPeriod.CollectionPeriod);
            incentive.ValidateMinimumRequiredAgreementVersion(command.PendingPaymentId, account, collectionCalendarPeriod.CollectionPeriod);

            await _domainRepository.Save(incentive);
        }
    }
}
