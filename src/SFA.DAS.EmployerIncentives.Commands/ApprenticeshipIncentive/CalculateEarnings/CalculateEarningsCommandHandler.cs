using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateEarnings
{
    public class CalculateEarningsCommandHandler : ICommandHandler<CalculateEarningsCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public CalculateEarningsCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository, 
            ICollectionCalendarService collectionCalendarService)
        {
            _domainRepository = domainRepository;
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(CalculateEarningsCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);

            incentive.CalculateEarnings(await _collectionCalendarService.Get());

            await _domainRepository.Save(incentive);
        }
    }
}
