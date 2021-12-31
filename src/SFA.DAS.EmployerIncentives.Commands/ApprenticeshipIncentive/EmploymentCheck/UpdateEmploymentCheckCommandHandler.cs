using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck
{
    public class UpdateEmploymentCheckCommandHandler : ICommandHandler<UpdateEmploymentCheckCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;

        public UpdateEmploymentCheckCommandHandler(IApprenticeshipIncentiveDomainRepository incentiveDomainRepository)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
        }

        public async Task Handle(UpdateEmploymentCheckCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _incentiveDomainRepository.FindByEmploymentCheckId(command.CorrelationId);

            if (incentive == null)
            {
                return;
            }

            incentive.UpdateEmploymentCheck(new EmploymentCheckResult(command.CorrelationId, command.Result, command.DateChecked));

            await _incentiveDomainRepository.Save(incentive);
        }
    }
}
