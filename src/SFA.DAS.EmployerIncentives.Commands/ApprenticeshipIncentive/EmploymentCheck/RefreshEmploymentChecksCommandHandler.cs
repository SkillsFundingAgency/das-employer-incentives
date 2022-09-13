using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck
{
    public class RefreshEmploymentChecksCommandHandler : ICommandHandler<RefreshEmploymentChecksCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly IDateTimeService _dateTimeService;

        public RefreshEmploymentChecksCommandHandler(
            IApprenticeshipIncentiveDomainRepository incentiveDomainRepository,
            IDateTimeService dateTimeService)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _dateTimeService = dateTimeService;
        }
        public async Task Handle(RefreshEmploymentChecksCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var incentivesWithLearning = await _incentiveDomainRepository.FindIncentivesWithLearningFound();

            foreach(var incentive in incentivesWithLearning)
            {
                incentive.RefreshEmploymentChecks(_dateTimeService);
                await _incentiveDomainRepository.Save(incentive);
            }
        }
    }

}
