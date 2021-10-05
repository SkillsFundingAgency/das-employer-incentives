using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.Withdraw
{
    public class WithdrawCommandHandler : ICommandHandler<WithdrawCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public WithdrawCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository,
            ICollectionCalendarService collectionCalendarService)
        {
            _domainRepository = domainRepository;
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(WithdrawCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId);
            if(incentive == null)
            {
                return;
            }
            
            incentive.Withdraw(command.WithdrawnBy, await _collectionCalendarService.Get());

            await _domainRepository.Save(incentive);
        }
    }
}
