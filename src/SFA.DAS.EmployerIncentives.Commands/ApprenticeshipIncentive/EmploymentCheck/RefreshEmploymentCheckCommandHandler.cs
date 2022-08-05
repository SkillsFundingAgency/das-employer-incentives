using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck
{
    public class RefreshEmploymentCheckCommandHandler : ICommandHandler<RefreshEmploymentCheckCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly IDateTimeService _dateTimeService;

        public RefreshEmploymentCheckCommandHandler(
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            IDateTimeService dateTimeService)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _dateTimeService = dateTimeService;
        }
        public async Task Handle(RefreshEmploymentCheckCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var incentive = await _incentiveDomainRepository.FindByUlnWithinAccountLegalEntity(command.ULN, command.AccountLegalEntityId);

            if (incentive == null)
            {
                return;
            }

            incentive.RefreshEmploymentChecks(
                _dateTimeService,
                new ServiceRequest(
                    command.ServiceRequestTaskId, 
                    command.DecisionReference, 
                    command.ServiceRequestCreated));

            await _incentiveDomainRepository.Save(incentive);
        }
    }

}
