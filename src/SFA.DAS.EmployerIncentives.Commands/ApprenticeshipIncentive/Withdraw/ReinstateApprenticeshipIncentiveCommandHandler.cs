using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.Withdraw
{
    public class ReinstateApprenticeshipIncentiveCommandHandler : ICommandHandler<ReinstateApprenticeshipIncentiveCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        
        public ReinstateApprenticeshipIncentiveCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(ReinstateApprenticeshipIncentiveCommand command, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
