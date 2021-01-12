using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.Withdraw
{
    public class WithdrawCommandHandler : ICommandHandler<WithdrawCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
     
        public WithdrawCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(WithdrawCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId);
            if(incentive == null)
            {
                return;
            }

            incentive.Delete();
            
            await _domainRepository.Save(incentive);
        }
    }
}
