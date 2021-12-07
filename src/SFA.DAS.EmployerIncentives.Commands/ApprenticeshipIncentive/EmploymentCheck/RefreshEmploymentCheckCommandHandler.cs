using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck
{
    public class RefreshEmploymentCheckCommandHandler : ICommandHandler<RefreshEmploymentCheckCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;

        public RefreshEmploymentCheckCommandHandler(IApprenticeshipIncentiveDomainRepository incentiveDomainRepository)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
        }
        public async Task Handle(RefreshEmploymentCheckCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var incentive = await _incentiveDomainRepository.FindByUlnWithinAccountLegalEntity(command.ULN, command.AccountLegalEntityId);

            if (incentive == null)
            {
                return;
            }

            incentive.AddEmploymentChecks(
                new ServiceRequest(
                    command.ServiceRequestTaskId, 
                    command.DecisionReference, 
                    command.ServiceRequestCreated));
        }
    }

}
