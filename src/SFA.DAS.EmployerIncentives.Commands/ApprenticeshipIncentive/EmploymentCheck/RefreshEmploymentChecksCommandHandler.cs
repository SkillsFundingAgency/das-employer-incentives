using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck
{
    public class RefreshEmploymentChecksCommandHandler : ICommandHandler<RefreshEmploymentChecksCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _incentiveDomainRepository;
        private readonly ICommandPublisher _commandPublisher;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public RefreshEmploymentChecksCommandHandler(IApprenticeshipIncentiveDomainRepository incentiveDomainRepository, 
                                                     ICommandPublisher commandPublisher,
                                                     ICollectionCalendarService collectionCalendarService)
        {
            _incentiveDomainRepository = incentiveDomainRepository;
            _commandPublisher = commandPublisher;
            _collectionCalendarService = collectionCalendarService;
        }
        public async Task Handle(RefreshEmploymentChecksCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await ActivePeriodInProgress())
            {
                return;
            }

            var incentivesWithLearning = await _incentiveDomainRepository.FindIncentivesWithLearningFound();

            foreach(var incentive in incentivesWithLearning)
            {
                if (!incentive.HasEmploymentChecks)
                {
                    incentive.AddEmploymentChecks();
                    await _incentiveDomainRepository.Save(incentive);
                }

                await _commandPublisher.Publish(new SendEmploymentCheckRequestsCommand(incentive.Id));
            }
        }


        private async Task<bool> ActivePeriodInProgress()
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            var activePeriod = collectionCalendar.GetActivePeriod();
            return activePeriod.PeriodEndInProgress;
        }
    }

}
