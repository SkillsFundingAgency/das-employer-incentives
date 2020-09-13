using System.Threading;
using System.Threading.Tasks;
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

        public CalculateEarningsCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository, 
            IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            _domainRepository = domainRepository;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
        }

        public async Task Handle(CalculateEarningsCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);

            var paymentProfiles = await _incentivePaymentProfilesService.Get();
            incentive.CalculateEarnings(paymentProfiles);

            await _domainRepository.Save(incentive);
        }
    }
}
