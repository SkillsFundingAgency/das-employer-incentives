using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
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

        public ValidatePendingPaymentCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository,
            IAccountDomainRepository accountDomainRepository,
            ICollectionCalendarService collectionCalendarService,
            ILearnerDomainRepository learnerDomainRepository)
        {
            _domainRepository = domainRepository;
            _accountDomainRepository = accountDomainRepository;
            _collectionCalendarService = collectionCalendarService;
            _learnerDomainRepository = learnerDomainRepository;
        }

        public async Task Handle(ValidatePendingPaymentCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);
            var account = await _accountDomainRepository.Find(incentive.Account.Id);
            var learner = await _learnerDomainRepository.Get(incentive);

            var calendar = await _collectionCalendarService.Get();
            var collectionPeriod = calendar.GetPeriod(command.CollectionYear, command.CollectionMonth);

            incentive.ValidatePendingPaymentBankDetails(command.PendingPaymentId, account, collectionPeriod);
            incentive.ValidateHasLearningRecord(command.PendingPaymentId, learner, collectionPeriod);
            incentive.ValidateIsInLearning(command.PendingPaymentId, learner, collectionPeriod);

            await _domainRepository.Save(incentive);
        }
    }
}
